namespace DentiFlow.Application.Interfaces;

/// <summary>
/// Servicio de chatbot IA para actuar como recepcionista virtual de la clínica.
/// Utiliza RAG (Retrieval-Augmented Generation) con datos reales de la clínica.
/// </summary>
public interface IChatbotService
{
    /// <summary>
    /// Envía una conversación al modelo de IA y obtiene la respuesta del asistente.
    /// El sistema usa datos de la clínica como contexto (RAG).
    /// </summary>
    /// <param name="systemPrompt">Prompt del sistema con el contexto RAG de la clínica.</param>
    /// <param name="messages">Historial de la conversación (role + content).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>La respuesta textual del asistente.</returns>
    Task<string> ChatAsync(string systemPrompt, IReadOnlyList<ChatMessage> messages, CancellationToken ct = default);

    /// <summary>Indica si el servicio está configurado (tiene API Key).</summary>
    bool IsConfigured { get; }
}

/// <summary>Mensaje individual en la conversación del chat.</summary>
public record ChatMessage(string Role, string Content);
