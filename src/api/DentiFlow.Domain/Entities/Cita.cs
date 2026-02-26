namespace DentiFlow.Domain.Entities;

public class Cita
{
    public Guid Id { get; set; }
    public Guid ClinicaId { get; set; }
    public Guid DentistaId { get; set; }
    public Guid PacienteId { get; set; }
    public DateTime FechaHora { get; set; }
    public int DuracionMinutos { get; set; } = 30;
    public string? Motivo { get; set; }
    public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;
    public string? MercadoPagoPaymentId { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Clinica Clinica { get; set; } = null!;
    public Dentista Dentista { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
}

public enum EstadoCita
{
    Pendiente,
    Confirmada,
    Pagada,
    EnProgreso,
    Completada,
    Cancelada,
    NoAsistio
}
