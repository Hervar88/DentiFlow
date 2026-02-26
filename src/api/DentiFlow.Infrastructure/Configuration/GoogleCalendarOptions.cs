namespace DentiFlow.Infrastructure.Configuration;

public class GoogleCalendarOptions
{
    public const string SectionName = "GoogleCalendar";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
