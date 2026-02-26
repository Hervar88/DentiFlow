using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentiFlow.Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly DentiFlowDbContext _db;

    public PacienteRepository(DentiFlowDbContext db) => _db = db;

    public async Task<Paciente?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Pacientes.FindAsync([id], ct);

    public async Task<List<Paciente>> GetByClinicaAsync(Guid clinicaId, CancellationToken ct = default)
        => await _db.Pacientes
            .Where(p => p.ClinicaId == clinicaId)
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .ToListAsync(ct);

    public async Task<Paciente> CreateAsync(Paciente paciente, CancellationToken ct = default)
    {
        paciente.Id = Guid.NewGuid();
        _db.Pacientes.Add(paciente);
        await _db.SaveChangesAsync(ct);
        return paciente;
    }

    public async Task UpdateAsync(Paciente paciente, CancellationToken ct = default)
    {
        _db.Pacientes.Update(paciente);
        await _db.SaveChangesAsync(ct);
    }
}
