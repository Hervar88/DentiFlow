namespace DentiFlow.Infrastructure.Configuration;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>API Key de OpenAI (sk-...).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Modelo a utilizar (gpt-4o-mini por defecto para bajo costo).</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Límite de tokens para la respuesta del asistente.</summary>
    public int MaxTokens { get; set; } = 500;
}
