using System.Text;
using DentiFlow.Application.Interfaces;
using DentiFlow.Domain.Interfaces;

namespace DentiFlow.Application.Services;

public class ChatService
{
    private readonly IClinicaRepository _clinicaRepo;
    private readonly ICitaRepository _citaRepo;
    private readonly IChatbotService _chatbot;

    public ChatService(
        IClinicaRepository clinicaRepo,
        ICitaRepository citaRepo,
        IChatbotService chatbot)
    {
        _clinicaRepo = clinicaRepo;
        _citaRepo = citaRepo;
        _chatbot = chatbot;
    }

    public bool IsConfigured => _chatbot.IsConfigured;

    /// <summary>
    /// Procesa un mensaje del usuario en el contexto de una clínica específica.
    /// Construye el prompt RAG con datos reales de la clínica y envía al modelo.
    /// </summary>
    public async Task<string> ProcessMessageAsync(
        string clinicaSlug,
        IReadOnlyList<ChatMessage> messages,
        CancellationToken ct = default)
    {
        var clinica = await _clinicaRepo.GetBySlugAsync(clinicaSlug, ct);
        if (clinica is null)
            return "No se encontró la clínica.";

        // Build RAG system prompt with real clinic data
        var systemPrompt = BuildSystemPrompt(clinica);

        return await _chatbot.ChatAsync(systemPrompt, messages, ct);
    }

    /// <summary>
    /// Construye el system prompt con datos reales de la clínica como contexto RAG.
    /// Esto evita que la IA invente datos — solo puede responder con la información proporcionada.
    /// </summary>
    private static string BuildSystemPrompt(Domain.Entities.Clinica clinica)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Eres la recepcionista virtual de una clínica dental. Tu nombre es Ana.");
        sb.AppendLine("Responde siempre en español, de forma amable, profesional y concisa.");
        sb.AppendLine("NUNCA inventes información que no esté en el contexto proporcionado.");
        sb.AppendLine("Si no sabes algo, sugiere al paciente contactar directamente a la clínica.");
        sb.AppendLine("Si el paciente quiere agendar una cita, guíalo para que use el botón 'Agendar Cita' en la página.");
        sb.AppendLine("No generes diagnósticos médicos ni recomendaciones de tratamiento específicas.");
        sb.AppendLine("Usa emojis moderadamente para hacer la conversación más amigable.");
        sb.AppendLine();
        sb.AppendLine("═══ DATOS DE LA CLÍNICA ═══");
        sb.AppendLine();
        sb.AppendLine($"Nombre: {clinica.Nombre}");

        if (!string.IsNullOrWhiteSpace(clinica.Descripcion))
            sb.AppendLine($"Descripción: {clinica.Descripcion}");

        if (!string.IsNullOrWhiteSpace(clinica.Direccion))
            sb.AppendLine($"Dirección: {clinica.Direccion}");

        if (!string.IsNullOrWhiteSpace(clinica.Telefono))
            sb.AppendLine($"Teléfono: {clinica.Telefono}");

        if (clinica.Especialidades.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("═══ SERVICIOS DISPONIBLES ═══");
            foreach (var esp in clinica.Especialidades)
            {
                sb.AppendLine($"• {esp}");
            }
        }

        if (clinica.Dentistas.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("═══ EQUIPO MÉDICo ═══");
            foreach (var d in clinica.Dentistas)
            {
                var especialidad = !string.IsNullOrWhiteSpace(d.Especialidad) ? d.Especialidad : "Odontología General";
                sb.AppendLine($"• Dr. {d.Nombre} {d.Apellido} — Especialidad: {especialidad}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("═══ INSTRUCCIONES ADICIONALES ═══");
        sb.AppendLine("• Horario de atención: Lunes a Viernes 9:00–19:00, Sábados 9:00–14:00");
        sb.AppendLine("• Para agendar cita, el paciente debe usar el botón 'Agendar Cita' de la página web");
        sb.AppendLine("• Se aceptan pagos con tarjeta, transferencia y efectivo");
        sb.AppendLine("• El anticipo para confirmar cita se puede pagar en línea con Mercado Pago");
        sb.AppendLine("• Las confirmaciones de cita se envían por WhatsApp");

        return sb.ToString();
    }
}
