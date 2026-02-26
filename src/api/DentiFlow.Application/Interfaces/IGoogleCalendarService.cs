using DentiFlow.Domain.Entities;

namespace DentiFlow.Application.Interfaces;

public interface IGoogleCalendarService
{
    /// <summary>
    /// Generates the Google OAuth2 authorization URL for a dentist to connect their calendar.
    /// </summary>
    string GetAuthorizationUrl(Guid dentistaId);

    /// <summary>
    /// Exchanges the authorization code for tokens and stores them on the dentist.
    /// </summary>
    Task HandleCallbackAsync(string code, Guid dentistaId, CancellationToken ct = default);

    /// <summary>
    /// Creates or updates a Google Calendar event for the given appointment.
    /// Returns the Google Calendar Event ID.
    /// </summary>
    Task<string?> SyncAppointmentAsync(Cita cita, CancellationToken ct = default);

    /// <summary>
    /// Deletes a Google Calendar event when an appointment is cancelled.
    /// </summary>
    Task DeleteAppointmentEventAsync(Cita cita, CancellationToken ct = default);

    /// <summary>
    /// Registers a webhook (push notification channel) for a dentist's calendar.
    /// </summary>
    Task RegisterWebhookAsync(Guid dentistaId, string webhookUrl, CancellationToken ct = default);

    /// <summary>
    /// Handles incoming webhook from Google Calendar — syncs changes back to DentiFlow.
    /// </summary>
    Task HandleWebhookAsync(string channelId, string resourceId, CancellationToken ct = default);

    /// <summary>
    /// Disconnects Google Calendar for a dentist (revokes token and clears stored data).
    /// </summary>
    Task DisconnectAsync(Guid dentistaId, CancellationToken ct = default);

    /// <summary>
    /// Returns the Google Calendar connection status for a dentist.
    /// </summary>
    Task<GoogleCalendarStatus> GetStatusAsync(Guid dentistaId, CancellationToken ct = default);
}

public record GoogleCalendarStatus(
    bool Connected,
    string? GoogleEmail,
    DateTime? TokenExpiry);
