using DentiFlow.Application.DTOs;
using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;

namespace DentiFlow.Application.Services;

public class DentistaService
{
    private readonly IDentistaRepository _repo;

    public DentistaService(IDentistaRepository repo) => _repo = repo;

    public async Task<List<DentistaDto>> GetByClinicaAsync(Guid clinicaId, CancellationToken ct = default)
    {
        var dentistas = await _repo.GetByClinicaAsync(clinicaId, ct);
        return dentistas.Select(d => new DentistaDto(
            d.Id, d.ClinicaId, d.Nombre, d.Apellido, d.Email, d.Especialidad, d.Telefono, d.CreatedAt
        )).ToList();
    }

    public async Task<DentistaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _repo.GetByIdAsync(id, ct);
        if (d is null) return null;
        return new DentistaDto(d.Id, d.ClinicaId, d.Nombre, d.Apellido, d.Email, d.Especialidad, d.Telefono, d.CreatedAt);
    }

    public async Task<DentistaDto> CreateAsync(CrearDentistaRequest request, CancellationToken ct = default)
    {
        var dentista = await _repo.CreateAsync(new Dentista
        {
            ClinicaId = request.ClinicaId,
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            Especialidad = request.Especialidad,
            Telefono = request.Telefono,
        }, ct);

        return new DentistaDto(
            dentista.Id, dentista.ClinicaId, dentista.Nombre, dentista.Apellido,
            dentista.Email, dentista.Especialidad, dentista.Telefono, dentista.CreatedAt);
    }

    public async Task<DentistaDto?> UpdateAsync(Guid id, ActualizarDentistaRequest request, CancellationToken ct = default)
    {
        var dentista = await _repo.GetByIdAsync(id, ct);
        if (dentista is null) return null;

        dentista.Nombre = request.Nombre;
        dentista.Apellido = request.Apellido;
        dentista.Email = request.Email;
        dentista.Especialidad = request.Especialidad;
        dentista.Telefono = request.Telefono;

        await _repo.UpdateAsync(dentista, ct);

        return new DentistaDto(
            dentista.Id, dentista.ClinicaId, dentista.Nombre, dentista.Apellido,
            dentista.Email, dentista.Especialidad, dentista.Telefono, dentista.CreatedAt);
    }
}
