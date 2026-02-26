using DentiFlow.Domain.Entities;

namespace DentiFlow.Application.DTOs;

public record CrearCitaRequest(
    Guid ClinicaId,
    Guid DentistaId,
    string NombrePaciente,
    string ApellidoPaciente,
    string? EmailPaciente,
    string? TelefonoPaciente,
    DateTime FechaHora,
    int DuracionMinutos = 30,
    string? Motivo = null);

public record ActualizarEstadoCitaRequest(string Estado);

public record CitaDto(
    Guid Id,
    DateTime FechaHora,
    int DuracionMinutos,
    string? Motivo,
    string Estado,
    string NombreDentista,
    string NombrePaciente,
    DateTime CreatedAt);

public static class CitaMapper
{
    public static CitaDto ToDto(Cita cita) => new(
        cita.Id,
        cita.FechaHora,
        cita.DuracionMinutos,
        cita.Motivo,
        cita.Estado.ToString(),
        $"{cita.Dentista.Nombre} {cita.Dentista.Apellido}",
        $"{cita.Paciente.Nombre} {cita.Paciente.Apellido}",
        cita.CreatedAt);
}
