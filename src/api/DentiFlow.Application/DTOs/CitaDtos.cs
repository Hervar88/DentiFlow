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
    string? MercadoPagoPaymentId,
    bool PuedeGenerarPago,
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
        cita.MercadoPagoPaymentId,
        // Se puede generar pago si la cita está Pendiente o Confirmada y no tiene pago real
        cita.Estado is EstadoCita.Pendiente or EstadoCita.Confirmada
            && (string.IsNullOrEmpty(cita.MercadoPagoPaymentId) || cita.MercadoPagoPaymentId.StartsWith("pref_")),
        cita.CreatedAt);
}
