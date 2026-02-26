namespace DentiFlow.Infrastructure.Configuration;

public class MercadoPagoOptions
{
    public const string SectionName = "MercadoPago";

    /// <summary>Access Token de producción o sandbox de Mercado Pago.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Secret para validar las notificaciones IPN/webhook.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>URL pública base para construir las URLs de retorno (ej: https://tudominio.com).</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>Monto del anticipo en MXN.</summary>
    public decimal AnticipoMonto { get; set; } = 500m;
}
