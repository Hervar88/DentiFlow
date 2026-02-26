using DentiFlow.Domain.Entities;

namespace DentiFlow.Application.Interfaces;

/// <summary>
/// Servicio para envío de notificaciones WhatsApp a pacientes mediante Twilio.
/// </summary>
public interface IWhatsAppService
{
    /// <summary>Envía confirmación de cita al paciente.</summary>
    Task SendBookingConfirmationAsync(Cita cita, CancellationToken ct = default);

    /// <summary>Envía recordatorio de cita (24h antes).</summary>
    Task SendReminderAsync(Cita cita, CancellationToken ct = default);

    /// <summary>Envía notificación de cancelación al paciente.</summary>
    Task SendCancellationAsync(Cita cita, CancellationToken ct = default);

    /// <summary>Envía notificación de cambio de estado al paciente.</summary>
    Task SendStatusUpdateAsync(Cita cita, EstadoCita nuevoEstado, CancellationToken ct = default);

    /// <summary>Indica si el servicio está configurado (tiene AccountSid y AuthToken).</summary>
    bool IsConfigured { get; }
}
