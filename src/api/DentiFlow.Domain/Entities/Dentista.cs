namespace DentiFlow.Domain.Entities;

public class Dentista
{
    public Guid Id { get; set; }
    public Guid ClinicaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public string? Telefono { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Google Calendar OAuth2
    public bool GoogleCalendarConnected { get; set; }
    public string? GoogleCalendarRefreshToken { get; set; }
    public string? GoogleCalendarAccessToken { get; set; }
    public DateTime? GoogleCalendarTokenExpiry { get; set; }
    public string? GoogleCalendarEmail { get; set; }

    // Navigation
    public Clinica Clinica { get; set; } = null!;
    public ICollection<Cita> Citas { get; set; } = [];
}
