namespace DentiFlow.Application.DTOs;

public record CrearPacienteRequest(
    Guid ClinicaId,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Notas);

public record ActualizarPacienteRequest(
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Notas);

public record PacienteDto(
    Guid Id,
    Guid ClinicaId,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Notas,
    DateTime CreatedAt);
