using DentiFlow.Domain.Entities;

namespace DentiFlow.Domain.Interfaces;

public interface IPacienteRepository
{
    Task<Paciente?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Paciente>> GetByClinicaAsync(Guid clinicaId, CancellationToken ct = default);
    Task<Paciente> CreateAsync(Paciente paciente, CancellationToken ct = default);
    Task UpdateAsync(Paciente paciente, CancellationToken ct = default);
}
