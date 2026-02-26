using DentiFlow.Application.Interfaces;
using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DentiFlow.Infrastructure.ExternalServices;

public class GoogleCalendarServiceImpl : IGoogleCalendarService
{
    private readonly GoogleCalendarOptions _options;
    private readonly IDentistaRepository _dentistaRepo;
    private readonly ICitaRepository _citaRepo;
    private readonly ILogger<GoogleCalendarServiceImpl> _logger;

    private static readonly string[] Scopes =
    [
        CalendarService.Scope.CalendarEvents,
        "https://www.googleapis.com/auth/userinfo.email"
    ];

    public GoogleCalendarServiceImpl(
        IOptions<GoogleCalendarOptions> options,
        IDentistaRepository dentistaRepo,
        ICitaRepository citaRepo,
        ILogger<GoogleCalendarServiceImpl> logger)
    {
        _options = options.Value;
        _dentistaRepo = dentistaRepo;
        _citaRepo = citaRepo;
        _logger = logger;
    }

    // ── OAuth2 Flow ──────────────────────────────────────────────

    public string GetAuthorizationUrl(Guid dentistaId)
    {
        var flow = CreateFlow();
        var url = flow.CreateAuthorizationCodeRequest(_options.RedirectUri);
        url.State = dentistaId.ToString();
        url.Scope = string.Join(" ", Scopes);
        // access_type=offline and prompt=consent to always get refresh token
        url.ResponseType = "code";
        return url.Build().AbsoluteUri + "&access_type=offline&prompt=consent";
    }

    public async Task HandleCallbackAsync(string code, Guid dentistaId, CancellationToken ct = default)
    {
        var dentista = await _dentistaRepo.GetByIdAsync(dentistaId, ct)
            ?? throw new InvalidOperationException("Dentista no encontrado.");

        var flow = CreateFlow();
        var tokenResponse = await flow.ExchangeCodeForTokenAsync(
            dentistaId.ToString(), code, _options.RedirectUri, ct);

        // Get the Google email
        var credential = new UserCredential(flow, dentistaId.ToString(), tokenResponse);
        string? googleEmail = null;

        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            var userInfo = await httpClient.GetStringAsync(
                "https://www.googleapis.com/oauth2/v2/userinfo", ct);
            var emailMatch = System.Text.RegularExpressions.Regex.Match(userInfo, "\"email\"\\s*:\\s*\"([^\"]+)\"");
            if (emailMatch.Success) googleEmail = emailMatch.Groups[1].Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve Google email for dentista {DentistaId}", dentistaId);
        }

        // Store tokens on the dentist entity
        dentista.GoogleCalendarConnected = true;
        dentista.GoogleCalendarAccessToken = tokenResponse.AccessToken;
        dentista.GoogleCalendarRefreshToken = tokenResponse.RefreshToken ?? dentista.GoogleCalendarRefreshToken;
        dentista.GoogleCalendarTokenExpiry = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);
        dentista.GoogleCalendarEmail = googleEmail;

        await _dentistaRepo.UpdateAsync(dentista, ct);

        _logger.LogInformation("Google Calendar connected for dentista {DentistaId} ({Email})",
            dentistaId, googleEmail);
    }

    // ── Calendar Sync ────────────────────────────────────────────

    public async Task<string?> SyncAppointmentAsync(Cita cita, CancellationToken ct = default)
    {
        var dentista = await _dentistaRepo.GetByIdAsync(cita.DentistaId, ct);
        if (dentista is null || !dentista.GoogleCalendarConnected || string.IsNullOrEmpty(dentista.GoogleCalendarRefreshToken))
        {
            _logger.LogDebug("Dentista {DentistaId} not connected to Google Calendar, skipping sync", cita.DentistaId);
            return null;
        }

        try
        {
            var calendarService = await CreateCalendarServiceAsync(dentista, ct);

            var calendarEvent = new Event
            {
                Summary = $"🦷 Cita: {cita.Paciente?.Nombre} {cita.Paciente?.Apellido}",
                Description = BuildEventDescription(cita),
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(cita.FechaHora, TimeSpan.Zero),
                    TimeZone = "America/Mexico_City"
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(
                        cita.FechaHora.AddMinutes(cita.DuracionMinutos), TimeSpan.Zero),
                    TimeZone = "America/Mexico_City"
                },
                ColorId = GetColorForEstado(cita.Estado),
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides =
                    [
                        new EventReminder { Method = "popup", Minutes = 30 },
                        new EventReminder { Method = "email", Minutes = 60 }
                    ]
                }
            };

            if (!string.IsNullOrEmpty(cita.GoogleCalendarEventId))
            {
                // Update existing event
                var request = calendarService.Events.Update(calendarEvent, "primary", cita.GoogleCalendarEventId);
                var updated = await request.ExecuteAsync(ct);
                _logger.LogInformation("Updated Google Calendar event {EventId} for cita {CitaId}",
                    updated.Id, cita.Id);
                return updated.Id;
            }
            else
            {
                // Create new event
                var request = calendarService.Events.Insert(calendarEvent, "primary");
                var created = await request.ExecuteAsync(ct);
                _logger.LogInformation("Created Google Calendar event {EventId} for cita {CitaId}",
                    created.Id, cita.Id);
                return created.Id;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync cita {CitaId} to Google Calendar for dentista {DentistaId}",
                cita.Id, cita.DentistaId);
            return null;
        }
    }

    public async Task DeleteAppointmentEventAsync(Cita cita, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(cita.GoogleCalendarEventId)) return;

        var dentista = await _dentistaRepo.GetByIdAsync(cita.DentistaId, ct);
        if (dentista is null || !dentista.GoogleCalendarConnected || string.IsNullOrEmpty(dentista.GoogleCalendarRefreshToken))
            return;

        try
        {
            var calendarService = await CreateCalendarServiceAsync(dentista, ct);
            await calendarService.Events.Delete("primary", cita.GoogleCalendarEventId).ExecuteAsync(ct);
            _logger.LogInformation("Deleted Google Calendar event {EventId} for cita {CitaId}",
                cita.GoogleCalendarEventId, cita.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Google Calendar event for cita {CitaId}", cita.Id);
        }
    }

    // ── Connection Management ──────────────────────────────────

    public async Task RegisterWebhookAsync(Guid dentistaId, string webhookUrl, CancellationToken ct = default)
    {
        var dentista = await _dentistaRepo.GetByIdAsync(dentistaId, ct);
        if (dentista is null || !dentista.GoogleCalendarConnected) return;

        try
        {
            var calendarService = await CreateCalendarServiceAsync(dentista, ct);
            var channel = new Channel
            {
                Id = $"dentiflow-{dentistaId}",
                Type = "web_hook",
                Address = webhookUrl,
                Expiration = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeMilliseconds()
            };

            await calendarService.Events.Watch(channel, "primary").ExecuteAsync(ct);
            _logger.LogInformation("Registered Google Calendar webhook for dentista {DentistaId}", dentistaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register webhook for dentista {DentistaId}", dentistaId);
        }
    }

    public async Task HandleWebhookAsync(string channelId, string resourceId, CancellationToken ct = default)
    {
        // channelId format: "dentiflow-{dentistaId}"
        if (!channelId.StartsWith("dentiflow-")) return;
        var dentistaIdStr = channelId["dentiflow-".Length..];
        if (!Guid.TryParse(dentistaIdStr, out var dentistaId)) return;

        var dentista = await _dentistaRepo.GetByIdAsync(dentistaId, ct);
        if (dentista is null || !dentista.GoogleCalendarConnected) return;

        try
        {
            var calendarService = await CreateCalendarServiceAsync(dentista, ct);

            // Get recent events updated in the last 5 minutes
            var request = calendarService.Events.List("primary");
            request.UpdatedMinDateTimeOffset = DateTimeOffset.UtcNow.AddMinutes(-5);
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.Updated;

            var events = await request.ExecuteAsync(ct);

            foreach (var gcalEvent in events.Items ?? [])
            {
                var cita = await _citaRepo.GetByGoogleCalendarEventIdAsync(gcalEvent.Id, ct);
                if (cita is null) continue;

                // Sync time changes back from Google Calendar to DentiFlow
                if (gcalEvent.Start?.DateTimeDateTimeOffset is not null)
                {
                    var newStart = gcalEvent.Start.DateTimeDateTimeOffset.Value.UtcDateTime;
                    if (cita.FechaHora != newStart)
                    {
                        _logger.LogInformation(
                            "Google Calendar event {EventId} moved: {Old} → {New} for cita {CitaId}",
                            gcalEvent.Id, cita.FechaHora, newStart, cita.Id);

                        cita.FechaHora = newStart;

                        if (gcalEvent.End?.DateTimeDateTimeOffset is not null)
                        {
                            var newEnd = gcalEvent.End.DateTimeDateTimeOffset.Value.UtcDateTime;
                            cita.DuracionMinutos = (int)(newEnd - newStart).TotalMinutes;
                        }

                        await _citaRepo.UpdateAsync(cita, ct);
                    }
                }

                // If event was cancelled in Google, cancel in DentiFlow
                if (gcalEvent.Status == "cancelled" && cita.Estado != EstadoCita.Cancelada)
                {
                    cita.Estado = EstadoCita.Cancelada;
                    cita.GoogleCalendarEventId = null;
                    await _citaRepo.UpdateAsync(cita, ct);
                    _logger.LogInformation("Cita {CitaId} cancelled via Google Calendar", cita.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle webhook for dentista {DentistaId}", dentistaId);
        }
    }

    public async Task DisconnectAsync(Guid dentistaId, CancellationToken ct = default)
    {
        var dentista = await _dentistaRepo.GetByIdAsync(dentistaId, ct)
            ?? throw new InvalidOperationException("Dentista no encontrado.");

        // Try to revoke the token
        if (!string.IsNullOrEmpty(dentista.GoogleCalendarAccessToken))
        {
            try
            {
                var httpClient = new HttpClient();
                await httpClient.PostAsync(
                    $"https://oauth2.googleapis.com/revoke?token={dentista.GoogleCalendarAccessToken}",
                    null, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke Google token for dentista {DentistaId}", dentistaId);
            }
        }

        dentista.GoogleCalendarConnected = false;
        dentista.GoogleCalendarAccessToken = null;
        dentista.GoogleCalendarRefreshToken = null;
        dentista.GoogleCalendarTokenExpiry = null;
        dentista.GoogleCalendarEmail = null;

        await _dentistaRepo.UpdateAsync(dentista, ct);
        _logger.LogInformation("Google Calendar disconnected for dentista {DentistaId}", dentistaId);
    }

    public async Task<GoogleCalendarStatus> GetStatusAsync(Guid dentistaId, CancellationToken ct = default)
    {
        var dentista = await _dentistaRepo.GetByIdAsync(dentistaId, ct);
        if (dentista is null)
            return new GoogleCalendarStatus(false, null, null);

        return new GoogleCalendarStatus(
            dentista.GoogleCalendarConnected,
            dentista.GoogleCalendarEmail,
            dentista.GoogleCalendarTokenExpiry);
    }

    // ── Private Helpers ──────────────────────────────────────────

    private GoogleAuthorizationCodeFlow CreateFlow()
    {
        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
            },
            Scopes = Scopes,
            DataStore = new NullDataStore()
        });
    }

    private async Task<CalendarService> CreateCalendarServiceAsync(Dentista dentista, CancellationToken ct)
    {
        var flow = CreateFlow();
        var tokenResponse = new TokenResponse
        {
            AccessToken = dentista.GoogleCalendarAccessToken,
            RefreshToken = dentista.GoogleCalendarRefreshToken,
            ExpiresInSeconds = dentista.GoogleCalendarTokenExpiry.HasValue
                ? (long)(dentista.GoogleCalendarTokenExpiry.Value - DateTime.UtcNow).TotalSeconds
                : 0,
            IssuedUtc = DateTime.UtcNow
        };

        var credential = new UserCredential(flow, dentista.Id.ToString(), tokenResponse);

        // Force refresh if token is expired or about to expire
        if (tokenResponse.ExpiresInSeconds <= 60)
        {
            var refreshed = await credential.RefreshTokenAsync(ct);
            if (refreshed)
            {
                // Persist the new tokens
                dentista.GoogleCalendarAccessToken = credential.Token.AccessToken;
                dentista.GoogleCalendarTokenExpiry = credential.Token.IssuedUtc
                    .AddSeconds(credential.Token.ExpiresInSeconds ?? 3600);
                await _dentistaRepo.UpdateAsync(dentista, ct);
            }
        }

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "DentiFlow"
        });
    }

    private static string BuildEventDescription(Cita cita)
    {
        var lines = new List<string>
        {
            $"📋 Estado: {cita.Estado}",
            $"👤 Paciente: {cita.Paciente?.Nombre} {cita.Paciente?.Apellido}"
        };

        if (!string.IsNullOrEmpty(cita.Motivo))
            lines.Add($"💬 Motivo: {cita.Motivo}");

        lines.Add($"\n🔗 Gestionado por DentiFlow");

        return string.Join("\n", lines);
    }

    private static string GetColorForEstado(EstadoCita estado) => estado switch
    {
        EstadoCita.Pendiente => "5",   // Banana (yellow)
        EstadoCita.Confirmada => "7",  // Peacock (blue)
        EstadoCita.Pagada => "2",      // Sage (green)
        EstadoCita.EnProgreso => "3",  // Grape (purple)
        EstadoCita.Completada => "10", // Basil (dark green)
        EstadoCita.Cancelada => "11",  // Tomato (red)
        EstadoCita.NoAsistio => "8",   // Graphite (gray)
        _ => "0"
    };
}

/// <summary>
/// NullDataStore because we manage tokens manually in our database.
/// </summary>
internal class NullDataStore : IDataStore
{
    public Task ClearAsync() => Task.CompletedTask;
    public Task DeleteAsync<T>(string key) => Task.CompletedTask;
    public Task<T> GetAsync<T>(string key) => Task.FromResult(default(T)!);
    public Task StoreAsync<T>(string key, T value) => Task.CompletedTask;
}
