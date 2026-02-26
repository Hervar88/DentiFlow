using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentiFlow.Infrastructure.Repositories;

public class CitaRepository : ICitaRepository
{
    private readonly DentiFlowDbContext _db;

    public CitaRepository(DentiFlowDbContext db) => _db = db;

    public async Task<Cita?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Citas
            .Include(c => c.Dentista)
            .Include(c => c.Paciente)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Cita>> GetByClinicaAsync(Guid clinicaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
        => await _db.Citas
            .Include(c => c.Dentista)
            .Include(c => c.Paciente)
            .Where(c => c.ClinicaId == clinicaId && c.FechaHora >= desde && c.FechaHora <= hasta)
            .OrderBy(c => c.FechaHora)
            .ToListAsync(ct);

    public async Task<List<Cita>> GetByDentistaAsync(Guid dentistaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
        => await _db.Citas
            .Include(c => c.Paciente)
            .Where(c => c.DentistaId == dentistaId && c.FechaHora >= desde && c.FechaHora <= hasta)
            .OrderBy(c => c.FechaHora)
            .ToListAsync(ct);

    public async Task<Cita> CreateAsync(Cita cita, CancellationToken ct = default)
    {
        cita.Id = Guid.NewGuid();
        _db.Citas.Add(cita);
        await _db.SaveChangesAsync(ct);
        return cita;
    }

    public async Task UpdateAsync(Cita cita, CancellationToken ct = default)
    {
        _db.Citas.Update(cita);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExisteConflictoAsync(Guid dentistaId, DateTime fechaHora, int duracionMinutos, Guid? excluirCitaId = null, CancellationToken ct = default)
    {
        var fin = fechaHora.AddMinutes(duracionMinutos);

        return await _db.Citas
            .Where(c => c.DentistaId == dentistaId
                && c.Estado != EstadoCita.Cancelada
                && (excluirCitaId == null || c.Id != excluirCitaId)
                && c.FechaHora < fin
                && c.FechaHora.AddMinutes(c.DuracionMinutos) > fechaHora)
            .AnyAsync(ct);
    }
}
