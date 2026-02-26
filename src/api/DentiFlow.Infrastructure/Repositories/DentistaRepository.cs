using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentiFlow.Infrastructure.Repositories;

public class DentistaRepository : IDentistaRepository
{
    private readonly DentiFlowDbContext _db;

    public DentistaRepository(DentiFlowDbContext db) => _db = db;

    public async Task<Dentista?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Dentistas.FindAsync([id], ct);

    public async Task<List<Dentista>> GetByClinicaAsync(Guid clinicaId, CancellationToken ct = default)
        => await _db.Dentistas
            .Where(d => d.ClinicaId == clinicaId)
            .OrderBy(d => d.Apellido)
            .ThenBy(d => d.Nombre)
            .ToListAsync(ct);

    public async Task<Dentista> CreateAsync(Dentista dentista, CancellationToken ct = default)
    {
        dentista.Id = Guid.NewGuid();
        _db.Dentistas.Add(dentista);
        await _db.SaveChangesAsync(ct);
        return dentista;
    }

    public async Task UpdateAsync(Dentista dentista, CancellationToken ct = default)
    {
        _db.Dentistas.Update(dentista);
        await _db.SaveChangesAsync(ct);
    }
}
