using DentiFlow.Application;
using DentiFlow.Application.DTOs;
using DentiFlow.Application.Interfaces;
using DentiFlow.Application.Services;
using DentiFlow.Domain.Entities;
using DentiFlow.Infrastructure;
using DentiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Clean Architecture DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Auto-migrate + seed in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DentiFlowDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(app.Services);
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

// ══════════════════════════════════════════════════════════════
// ──── Clinica Endpoints ────
// ══════════════════════════════════════════════════════════════

app.MapGet("/clinica/{slug}", async (string slug, ClinicaService svc, CancellationToken ct) =>
{
    var profile = await svc.GetProfileBySlugAsync(slug, ct);
    return profile is not null ? Results.Ok(profile) : Results.NotFound();
})
.WithName("GetClinicaProfile")
.WithTags("Clinica");

// ══════════════════════════════════════════════════════════════
// ──── Dentista Endpoints ────
// ══════════════════════════════════════════════════════════════

app.MapGet("/dentistas", async (Guid clinicaId, DentistaService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetByClinicaAsync(clinicaId, ct)))
.WithName("GetDentistas")
.WithTags("Dentistas");

app.MapGet("/dentistas/{id:guid}", async (Guid id, DentistaService svc, CancellationToken ct) =>
{
    var dentista = await svc.GetByIdAsync(id, ct);
    return dentista is not null ? Results.Ok(dentista) : Results.NotFound();
})
.WithName("GetDentista")
.WithTags("Dentistas");

app.MapPost("/dentistas", async (CrearDentistaRequest request, DentistaService svc, CancellationToken ct) =>
{
    var dentista = await svc.CreateAsync(request, ct);
    return Results.Created($"/dentistas/{dentista.Id}", dentista);
})
.WithName("CreateDentista")
.WithTags("Dentistas");

app.MapPut("/dentistas/{id:guid}", async (Guid id, ActualizarDentistaRequest request, DentistaService svc, CancellationToken ct) =>
{
    var dentista = await svc.UpdateAsync(id, request, ct);
    return dentista is not null ? Results.Ok(dentista) : Results.NotFound();
})
.WithName("UpdateDentista")
.WithTags("Dentistas");

// ══════════════════════════════════════════════════════════════
// ──── Paciente Endpoints ────
// ══════════════════════════════════════════════════════════════

app.MapGet("/pacientes", async (Guid clinicaId, PacienteService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetByClinicaAsync(clinicaId, ct)))
.WithName("GetPacientes")
.WithTags("Pacientes");

app.MapGet("/pacientes/{id:guid}", async (Guid id, PacienteService svc, CancellationToken ct) =>
{
    var paciente = await svc.GetByIdAsync(id, ct);
    return paciente is not null ? Results.Ok(paciente) : Results.NotFound();
})
.WithName("GetPaciente")
.WithTags("Pacientes");

app.MapPost("/pacientes", async (CrearPacienteRequest request, PacienteService svc, CancellationToken ct) =>
{
    var paciente = await svc.CreateAsync(request, ct);
    return Results.Created($"/pacientes/{paciente.Id}", paciente);
})
.WithName("CreatePaciente")
.WithTags("Pacientes");

app.MapPut("/pacientes/{id:guid}", async (Guid id, ActualizarPacienteRequest request, PacienteService svc, CancellationToken ct) =>
{
    var paciente = await svc.UpdateAsync(id, request, ct);
    return paciente is not null ? Results.Ok(paciente) : Results.NotFound();
})
.WithName("UpdatePaciente")
.WithTags("Pacientes");

// ══════════════════════════════════════════════════════════════
// ──── Cita (Appointment) Endpoints ────
// ══════════════════════════════════════════════════════════════

app.MapGet("/appointments", async (Guid clinicaId, DateTime desde, DateTime hasta, CitaService svc, CancellationToken ct) =>
{
    var citas = await svc.GetByClinicaAsync(clinicaId, desde, hasta, ct);
    return Results.Ok(citas);
})
.WithName("GetAppointments")
.WithTags("Citas");

app.MapGet("/appointments/{id:guid}", async (Guid id, CitaService svc, CancellationToken ct) =>
{
    var cita = await svc.GetByIdAsync(id, ct);
    return cita is not null ? Results.Ok(cita) : Results.NotFound();
})
.WithName("GetAppointment")
.WithTags("Citas");

app.MapPost("/appointments/book", async (CrearCitaRequest request, CitaService svc, CancellationToken ct) =>
{
    try
    {
        var cita = await svc.BookAppointmentAsync(request, ct);
        return Results.Created($"/appointments/{cita.Id}", cita);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("BookAppointment")
.WithTags("Citas");

app.MapPatch("/appointments/{id:guid}/status", async (Guid id, ActualizarEstadoCitaRequest request, CitaService svc, CancellationToken ct) =>
{
    if (!Enum.TryParse<EstadoCita>(request.Estado, true, out var estado))
        return Results.BadRequest(new { error = $"Estado inválido. Valores: {string.Join(", ", Enum.GetNames<EstadoCita>())}" });

    var cita = await svc.UpdateEstadoAsync(id, estado, ct);
    return cita is not null ? Results.Ok(cita) : Results.NotFound();
})
.WithName("UpdateAppointmentStatus")
.WithTags("Citas");

app.MapDelete("/appointments/{id:guid}", async (Guid id, CitaService svc, CancellationToken ct) =>
{
    var cita = await svc.CancelAsync(id, ct);
    return cita is not null ? Results.Ok(cita) : Results.NotFound();
})
.WithName("CancelAppointment")
.WithTags("Citas");

// ══════════════════════════════════════════════════════════════
// ──── Google Calendar Endpoints ────
// ══════════════════════════════════════════════════════════════

app.MapGet("/google-calendar/auth-url", (Guid dentistaId, IGoogleCalendarService gcSvc) =>
{
    var url = gcSvc.GetAuthorizationUrl(dentistaId);
    return Results.Ok(new { authUrl = url });
})
.WithName("GoogleCalendarAuthUrl")
.WithTags("GoogleCalendar");

app.MapGet("/google-calendar/callback", async (string code, string state, IGoogleCalendarService gcSvc, CancellationToken ct) =>
{
    if (!Guid.TryParse(state, out var dentistaId))
        return Results.BadRequest(new { error = "State inválido." });

    try
    {
        await gcSvc.HandleCallbackAsync(code, dentistaId, ct);
        // Redirect back to the dashboard with success
        return Results.Redirect("/dashboard?gcal=connected");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GoogleCalendarCallback")
.WithTags("GoogleCalendar");

app.MapGet("/google-calendar/status/{dentistaId:guid}", async (Guid dentistaId, IGoogleCalendarService gcSvc, CancellationToken ct) =>
{
    var status = await gcSvc.GetStatusAsync(dentistaId, ct);
    return Results.Ok(status);
})
.WithName("GoogleCalendarStatus")
.WithTags("GoogleCalendar");

app.MapDelete("/google-calendar/disconnect/{dentistaId:guid}", async (Guid dentistaId, IGoogleCalendarService gcSvc, CancellationToken ct) =>
{
    try
    {
        await gcSvc.DisconnectAsync(dentistaId, ct);
        return Results.Ok(new { message = "Google Calendar desconectado." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GoogleCalendarDisconnect")
.WithTags("GoogleCalendar");

app.MapPost("/google-calendar/webhook", async (HttpContext ctx, IGoogleCalendarService gcSvc, CancellationToken ct) =>
{
    // Google sends channel info in headers
    var channelId = ctx.Request.Headers["X-Goog-Channel-ID"].FirstOrDefault();
    var resourceId = ctx.Request.Headers["X-Goog-Resource-ID"].FirstOrDefault();

    if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(resourceId))
        return Results.BadRequest();

    await gcSvc.HandleWebhookAsync(channelId, resourceId, ct);
    return Results.Ok();
})
.WithName("GoogleCalendarWebhook")
.WithTags("GoogleCalendar");

// ──── Health Check ────
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
