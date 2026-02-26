using DentiFlow.Application.Interfaces;
using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Configuration;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DentiFlow.Infrastructure.ExternalServices;

public class MercadoPagoServiceImpl : IMercadoPagoService
{
    private readonly MercadoPagoOptions _options;
    private readonly ICitaRepository _citaRepo;
    private readonly ILogger<MercadoPagoServiceImpl> _logger;

    public MercadoPagoServiceImpl(
        IOptions<MercadoPagoOptions> options,
        ICitaRepository citaRepo,
        ILogger<MercadoPagoServiceImpl> logger)
    {
        _options = options.Value;
        _citaRepo = citaRepo;
        _logger = logger;

        // Configure the SDK if AccessToken is available
        if (IsConfigured)
        {
            MercadoPagoConfig.AccessToken = _options.AccessToken;
        }
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.AccessToken);

    public async Task<MercadoPagoPreferenceResult> CreatePreferenceAsync(Guid citaId, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Mercado Pago no está configurado. Agrega el AccessToken en la configuración.");

        var cita = await _citaRepo.GetByIdAsync(citaId, ct)
            ?? throw new InvalidOperationException("Cita no encontrada.");

        if (cita.Estado == EstadoCita.Cancelada)
            throw new InvalidOperationException("No se puede generar pago para una cita cancelada.");

        if (cita.Estado == EstadoCita.Pagada)
            throw new InvalidOperationException("Esta cita ya fue pagada.");

        var client = new PreferenceClient();

        var request = new PreferenceRequest
        {
            Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = $"Anticipo — Cita dental ({cita.Motivo ?? "Consulta general"})",
                    Description = $"Anticipo para cita el {cita.FechaHora:dd/MM/yyyy HH:mm} con Dr. {cita.Dentista?.Nombre} {cita.Dentista?.Apellido}",
                    Quantity = 1,
                    CurrencyId = "MXN",
                    UnitPrice = _options.AnticipoMonto,
                }
            },
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = $"{_options.PublicBaseUrl}/pago-exitoso?citaId={citaId}",
                Failure = $"{_options.PublicBaseUrl}/pago-fallido?citaId={citaId}",
                Pending = $"{_options.PublicBaseUrl}/pago-pendiente?citaId={citaId}",
            },
            AutoReturn = "approved",
            ExternalReference = citaId.ToString(),
            NotificationUrl = $"{_options.PublicBaseUrl}/api/payments/webhook",
            StatementDescriptor = "DENTIFLOW",
        };

        Preference preference;
        try
        {
            preference = await client.CreateAsync(request, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Mercado Pago preference for cita {CitaId}", citaId);
            throw new InvalidOperationException("Error al crear la preferencia de pago en Mercado Pago.", ex);
        }

        // Store the preference ID on the cita for tracking
        cita.MercadoPagoPaymentId = $"pref_{preference.Id}";
        await _citaRepo.UpdateAsync(cita, ct);

        _logger.LogInformation(
            "Created Mercado Pago preference {PreferenceId} for cita {CitaId}, amount {Amount} MXN",
            preference.Id, citaId, _options.AnticipoMonto);

        return new MercadoPagoPreferenceResult(
            preference.Id?.ToString() ?? "",
            preference.InitPoint ?? "",
            preference.SandboxInitPoint ?? "");
    }

    public async Task HandleWebhookAsync(string type, string dataId, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Mercado Pago webhook received but service is not configured.");
            return;
        }

        // Only process payment notifications
        if (type != "payment")
        {
            _logger.LogInformation("Ignoring Mercado Pago webhook type: {Type}", type);
            return;
        }

        if (!long.TryParse(dataId, out var paymentId))
        {
            _logger.LogWarning("Invalid payment ID in webhook: {DataId}", dataId);
            return;
        }

        try
        {
            MercadoPagoConfig.AccessToken = _options.AccessToken;
            var paymentClient = new PaymentClient();
            var payment = await paymentClient.GetAsync(paymentId, cancellationToken: ct);

            if (payment is null)
            {
                _logger.LogWarning("Payment {PaymentId} not found in Mercado Pago", paymentId);
                return;
            }

            _logger.LogInformation(
                "Mercado Pago webhook — Payment {PaymentId}, Status: {Status}, ExternalRef: {ExternalRef}",
                paymentId, payment.Status, payment.ExternalReference);

            // The ExternalReference is the citaId
            if (!Guid.TryParse(payment.ExternalReference, out var citaId))
            {
                _logger.LogWarning("Invalid external reference in payment: {ExternalRef}", payment.ExternalReference);
                return;
            }

            var cita = await _citaRepo.GetByIdAsync(citaId, ct);
            if (cita is null)
            {
                _logger.LogWarning("Cita {CitaId} not found for payment {PaymentId}", citaId, paymentId);
                return;
            }

            if (payment.Status == "approved")
            {
                cita.Estado = EstadoCita.Pagada;
                cita.MercadoPagoPaymentId = paymentId.ToString();
                await _citaRepo.UpdateAsync(cita, ct);

                _logger.LogInformation(
                    "Cita {CitaId} marked as Pagada via Mercado Pago payment {PaymentId}",
                    citaId, paymentId);
            }
            else
            {
                // Store the payment id for tracking even if not yet approved
                cita.MercadoPagoPaymentId = paymentId.ToString();
                await _citaRepo.UpdateAsync(cita, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Mercado Pago webhook for payment {DataId}", dataId);
        }
    }

    public async Task<MercadoPagoPaymentStatus?> GetPaymentStatusAsync(Guid citaId, CancellationToken ct = default)
    {
        var cita = await _citaRepo.GetByIdAsync(citaId, ct);
        if (cita is null) return null;

        // If no payment has been made yet
        if (string.IsNullOrWhiteSpace(cita.MercadoPagoPaymentId))
        {
            return new MercadoPagoPaymentStatus(null, "pending", null, null);
        }

        // If it starts with pref_ it's just a preference, not yet paid
        if (cita.MercadoPagoPaymentId.StartsWith("pref_"))
        {
            return new MercadoPagoPaymentStatus(null, "preference_created", null, null);
        }

        // Try to fetch the real payment status from MP
        if (IsConfigured && long.TryParse(cita.MercadoPagoPaymentId, out var paymentId))
        {
            try
            {
                MercadoPagoConfig.AccessToken = _options.AccessToken;
                var paymentClient = new PaymentClient();
                var payment = await paymentClient.GetAsync(paymentId, cancellationToken: ct);

                return new MercadoPagoPaymentStatus(
                    paymentId.ToString(),
                    payment?.Status ?? "unknown",
                    payment?.TransactionAmount,
                    payment?.DateApproved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment {PaymentId} from Mercado Pago", paymentId);
            }
        }

        // Fallback — return based on cita estado
        return new MercadoPagoPaymentStatus(
            cita.MercadoPagoPaymentId,
            cita.Estado == EstadoCita.Pagada ? "approved" : "unknown",
            null,
            null);
    }
}
