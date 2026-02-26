using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentiFlow.Infrastructure.Repositories;

public class ClinicaRepository : IClinicaRepository
{
    private readonly DentiFlowDbContext _db;

    public ClinicaRepository(DentiFlowDbContext db) => _db = db;

    public async Task<Clinica?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Clinicas.FindAsync([id], ct);

    public async Task<Clinica?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _db.Clinicas
            .Include(c => c.Dentistas)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<Clinica> CreateAsync(Clinica clinica, CancellationToken ct = default)
    {
        clinica.Id = Guid.NewGuid();
        _db.Clinicas.Add(clinica);
        await _db.SaveChangesAsync(ct);
        return clinica;
    }

    public async Task UpdateAsync(Clinica clinica, CancellationToken ct = default)
    {
        _db.Clinicas.Update(clinica);
        await _db.SaveChangesAsync(ct);
    }
}
