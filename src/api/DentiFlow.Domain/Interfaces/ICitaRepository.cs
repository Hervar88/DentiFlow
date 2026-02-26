using DentiFlow.Domain.Entities;

namespace DentiFlow.Domain.Interfaces;

public interface ICitaRepository
{
    Task<Cita?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Cita?> GetByGoogleCalendarEventIdAsync(string eventId, CancellationToken ct = default);
    Task<List<Cita>> GetByClinicaAsync(Guid clinicaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<List<Cita>> GetByDentistaAsync(Guid dentistaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Cita> CreateAsync(Cita cita, CancellationToken ct = default);
    Task UpdateAsync(Cita cita, CancellationToken ct = default);
    Task<bool> ExisteConflictoAsync(Guid dentistaId, DateTime fechaHora, int duracionMinutos, Guid? excluirCitaId = null, CancellationToken ct = default);
}
