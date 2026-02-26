using DentiFlow.Domain.Entities;

namespace DentiFlow.Domain.Interfaces;

public interface IClinicaRepository
{
    Task<Clinica?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Clinica?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Clinica> CreateAsync(Clinica clinica, CancellationToken ct = default);
    Task UpdateAsync(Clinica clinica, CancellationToken ct = default);
}
