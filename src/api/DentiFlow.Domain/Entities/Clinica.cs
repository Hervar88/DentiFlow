namespace DentiFlow.Domain.Entities;

public class Clinica
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Descripcion { get; set; }
    public List<string> Especialidades { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Dentista> Dentistas { get; set; } = [];
    public ICollection<Paciente> Pacientes { get; set; } = [];
}
