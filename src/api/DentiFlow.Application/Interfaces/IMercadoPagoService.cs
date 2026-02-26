namespace DentiFlow.Application.Interfaces;

/// <summary>
/// Servicio para integración con Mercado Pago Checkout Pro.
/// Permite crear preferencias de pago (anticipo) y procesar notificaciones de webhook.
/// </summary>
public interface IMercadoPagoService
{
    /// <summary>
    /// Crea una preferencia de Checkout Pro para el anticipo de una cita.
    /// Retorna la URL de inicio del checkout (init_point).
    /// </summary>
    Task<MercadoPagoPreferenceResult> CreatePreferenceAsync(Guid citaId, CancellationToken ct = default);

    /// <summary>
    /// Procesa una notificación de webhook de Mercado Pago.
    /// Si el pago fue aprobado, marca la cita como "Pagada".
    /// </summary>
    Task HandleWebhookAsync(string type, string dataId, CancellationToken ct = default);

    /// <summary>
    /// Consulta el estado de pago de una cita.
    /// </summary>
    Task<MercadoPagoPaymentStatus?> GetPaymentStatusAsync(Guid citaId, CancellationToken ct = default);

    /// <summary>
    /// Indica si el servicio está configurado (tiene AccessToken).
    /// </summary>
    bool IsConfigured { get; }
}

public record MercadoPagoPreferenceResult(
    string PreferenceId,
    string InitPoint,
    string SandboxInitPoint);

public record MercadoPagoPaymentStatus(
    string? PaymentId,
    string Status,
    decimal? AmountPaid,
    DateTime? PaidAt);
