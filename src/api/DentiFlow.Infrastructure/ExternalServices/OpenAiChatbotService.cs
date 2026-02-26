using DentiFlow.Application.Interfaces;
using DentiFlow.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace DentiFlow.Infrastructure.ExternalServices;

public class OpenAiChatbotService : IChatbotService
{
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiChatbotService> _logger;

    public OpenAiChatbotService(IOptions<OpenAiOptions> options, ILogger<OpenAiChatbotService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<string> ChatAsync(
        string systemPrompt,
        IReadOnlyList<Application.Interfaces.ChatMessage> messages,
        CancellationToken ct = default)
    {
        if (!IsConfigured)
            return "El chatbot no está configurado. Contacta a la clínica directamente.";

        try
        {
            var client = new ChatClient(_options.Model, _options.ApiKey);

            var chatMessages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage(systemPrompt)
            };

            foreach (var msg in messages)
            {
                chatMessages.Add(msg.Role.ToLowerInvariant() switch
                {
                    "user" => new UserChatMessage(msg.Content),
                    "assistant" => new AssistantChatMessage(msg.Content),
                    _ => new UserChatMessage(msg.Content)
                });
            }

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _options.MaxTokens,
                Temperature = 0.7f,
            };

            var completion = await client.CompleteChatAsync(chatMessages, options, ct);

            var response = completion.Value.Content[0].Text;

            _logger.LogInformation(
                "Chatbot response generated. Model: {Model}, Input tokens: {Input}, Output tokens: {Output}",
                _options.Model,
                completion.Value.Usage.InputTokenCount,
                completion.Value.Usage.OutputTokenCount);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return "Lo siento, no pude procesar tu mensaje en este momento. Por favor intenta de nuevo o contacta a la clínica directamente.";
        }
    }
}
