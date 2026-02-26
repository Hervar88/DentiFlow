using DentiFlow.Application.Interfaces;
using DentiFlow.Domain.Entities;
using DentiFlow.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace DentiFlow.Infrastructure.ExternalServices;

public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioWhatsAppService> _logger;

    public TwilioWhatsAppService(IOptions<TwilioOptions> options, ILogger<TwilioWhatsAppService> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (IsConfigured)
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        }
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.AccountSid) &&
        !string.IsNullOrWhiteSpace(_options.AuthToken) &&
        !string.IsNullOrWhiteSpace(_options.WhatsAppFromNumber);

    public async Task SendBookingConfirmationAsync(Cita cita, CancellationToken ct = default)
    {
        var telefono = cita.Paciente?.Telefono;
        if (!CanSend(telefono)) return;

        var dentistaNombre = cita.Dentista is not null
            ? $"Dr. {cita.Dentista.Nombre} {cita.Dentista.Apellido}"
            : "su dentista";

        var mensaje = $"""
            ✅ *Cita confirmada* — {_options.ClinicaNombre}

            Hola {cita.Paciente!.Nombre}, tu cita ha sido agendada:

            📅 *Fecha:* {cita.FechaHora:dddd d 'de' MMMM, yyyy}
            🕐 *Hora:* {cita.FechaHora:HH:mm} hrs
            👨‍⚕️ *Dentista:* {dentistaNombre}
            📝 *Motivo:* {cita.Motivo ?? "Consulta general"}

            Te esperamos. Si necesitas cancelar o reagendar, contáctanos con anticipación.
            """;

        await SendWhatsAppAsync(telefono!, mensaje.Trim());
    }

    public async Task SendReminderAsync(Cita cita, CancellationToken ct = default)
    {
        var telefono = cita.Paciente?.Telefono;
        if (!CanSend(telefono)) return;

        var dentistaNombre = cita.Dentista is not null
            ? $"Dr. {cita.Dentista.Nombre} {cita.Dentista.Apellido}"
            : "su dentista";

        var mensaje = $"""
            ⏰ *Recordatorio de cita* — {_options.ClinicaNombre}

            Hola {cita.Paciente!.Nombre}, te recordamos que tienes una cita:

            📅 *Fecha:* {cita.FechaHora:dddd d 'de' MMMM, yyyy}
            🕐 *Hora:* {cita.FechaHora:HH:mm} hrs
            👨‍⚕️ *Dentista:* {dentistaNombre}

            Por favor llega 10 minutos antes. ¡Te esperamos!
            """;

        await SendWhatsAppAsync(telefono!, mensaje.Trim());
    }

    public async Task SendCancellationAsync(Cita cita, CancellationToken ct = default)
    {
        var telefono = cita.Paciente?.Telefono;
        if (!CanSend(telefono)) return;

        var mensaje = $"""
            ❌ *Cita cancelada* — {_options.ClinicaNombre}

            Hola {cita.Paciente!.Nombre}, tu cita del {cita.FechaHora:dddd d 'de' MMMM} a las {cita.FechaHora:HH:mm} hrs ha sido cancelada.

            Si deseas reagendar, contáctanos o visita nuestra página para agendar una nueva cita.
            """;

        await SendWhatsAppAsync(telefono!, mensaje.Trim());
    }

    public async Task SendStatusUpdateAsync(Cita cita, EstadoCita nuevoEstado, CancellationToken ct = default)
    {
        var telefono = cita.Paciente?.Telefono;
        if (!CanSend(telefono)) return;

        var estadoTexto = nuevoEstado switch
        {
            EstadoCita.Confirmada => "✅ confirmada",
            EstadoCita.Pagada => "💰 pagada (anticipo recibido)",
            EstadoCita.EnProgreso => "🦷 en progreso",
            EstadoCita.Completada => "🎉 completada",
            _ => null
        };

        // Solo notificamos cambios relevantes para el paciente
        if (estadoTexto is null) return;

        var mensaje = $"""
            📋 *Actualización de cita* — {_options.ClinicaNombre}

            Hola {cita.Paciente!.Nombre}, tu cita del {cita.FechaHora:dddd d 'de' MMMM} a las {cita.FechaHora:HH:mm} hrs ahora está {estadoTexto}.
            """;

        await SendWhatsAppAsync(telefono!, mensaje.Trim());
    }

    // ── Private helpers ──

    private bool CanSend(string? telefono)
    {
        if (!IsConfigured)
        {
            _logger.LogDebug("WhatsApp notification skipped: Twilio not configured");
            return false;
        }

        if (string.IsNullOrWhiteSpace(telefono))
        {
            _logger.LogDebug("WhatsApp notification skipped: patient has no phone number");
            return false;
        }

        return true;
    }

    private async Task SendWhatsAppAsync(string toPhone, string body)
    {
        try
        {
            // Normalize phone number to WhatsApp format
            var whatsappTo = NormalizeWhatsAppNumber(toPhone);

            var message = await MessageResource.CreateAsync(
                from: new PhoneNumber(_options.WhatsAppFromNumber),
                to: new PhoneNumber(whatsappTo),
                body: body);

            _logger.LogInformation(
                "WhatsApp message sent to {Phone}, SID: {Sid}, Status: {Status}",
                whatsappTo, message.Sid, message.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {Phone}", toPhone);
            // Non-blocking: appointment operations should not fail because of WhatsApp
        }
    }

    /// <summary>
    /// Normaliza el número de teléfono al formato WhatsApp de Twilio: "whatsapp:+521234567890".
    /// Acepta formatos: "5512345678", "+525512345678", "whatsapp:+525512345678".
    /// Asume código de país México (+52) si no tiene prefijo internacional.
    /// </summary>
    private static string NormalizeWhatsAppNumber(string phone)
    {
        // Already in WhatsApp format
        if (phone.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            return phone;

        // Strip spaces, dashes, parentheses
        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // Add country code if missing (default: Mexico +52)
        if (!cleaned.StartsWith('+'))
        {
            cleaned = cleaned.Length == 10
                ? $"+52{cleaned}"   // 10 digits → Mexican mobile
                : $"+{cleaned}";    // Already has country code without +
        }

        return $"whatsapp:{cleaned}";
    }
}
