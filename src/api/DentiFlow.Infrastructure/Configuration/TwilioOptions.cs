namespace DentiFlow.Infrastructure.Configuration;

public class TwilioOptions
{
    public const string SectionName = "Twilio";

    /// <summary>Account SID de Twilio (empieza con AC...).</summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>Auth Token de Twilio.</summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>Número de WhatsApp de Twilio (ej: "whatsapp:+14155238886" para sandbox).</summary>
    public string WhatsAppFromNumber { get; set; } = string.Empty;

    /// <summary>Nombre de la clínica para incluir en los mensajes.</summary>
    public string ClinicaNombre { get; set; } = "Tu Clínica Dental";
}
