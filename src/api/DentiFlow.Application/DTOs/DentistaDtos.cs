namespace DentiFlow.Application.DTOs;

public record CrearDentistaRequest(
    Guid ClinicaId,
    string Nombre,
    string Apellido,
    string Email,
    string? Especialidad,
    string? Telefono);

public record ActualizarDentistaRequest(
    string Nombre,
    string Apellido,
    string Email,
    string? Especialidad,
    string? Telefono);

public record DentistaDto(
    Guid Id,
    Guid ClinicaId,
    string Nombre,
    string Apellido,
    string Email,
    string? Especialidad,
    string? Telefono,
    bool GoogleCalendarConnected,
    string? GoogleCalendarEmail,
    DateTime CreatedAt);
