namespace DentiFlow.Domain.Entities;

public class Paciente
{
    public Guid Id { get; set; }
    public Guid ClinicaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Clinica Clinica { get; set; } = null!;
    public ICollection<Cita> Citas { get; set; } = [];
}
