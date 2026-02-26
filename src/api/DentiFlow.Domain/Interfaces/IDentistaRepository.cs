using DentiFlow.Domain.Entities;

namespace DentiFlow.Domain.Interfaces;

public interface IDentistaRepository
{
    Task<Dentista?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Dentista>> GetByClinicaAsync(Guid clinicaId, CancellationToken ct = default);
    Task<Dentista> CreateAsync(Dentista dentista, CancellationToken ct = default);
    Task UpdateAsync(Dentista dentista, CancellationToken ct = default);
}
