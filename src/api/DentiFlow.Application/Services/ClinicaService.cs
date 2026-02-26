using DentiFlow.Application.DTOs;
using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;

namespace DentiFlow.Application.Services;

public class ClinicaService
{
    private readonly IClinicaRepository _clinicaRepo;

    public ClinicaService(IClinicaRepository clinicaRepo) => _clinicaRepo = clinicaRepo;

    public async Task<ClinicaProfileDto?> GetProfileBySlugAsync(string slug, CancellationToken ct = default)
    {
        var clinica = await _clinicaRepo.GetBySlugAsync(slug, ct);
        if (clinica is null) return null;

        return new ClinicaProfileDto(
            clinica.Id,
            clinica.Nombre,
            clinica.Slug,
            clinica.LogoUrl,
            clinica.Telefono,
            clinica.Direccion,
            clinica.Descripcion,
            clinica.Especialidades,
            clinica.Dentistas.Select(d => new DentistaResumenDto(
                d.Id, d.Nombre, d.Apellido, d.Especialidad)).ToList());
    }
}
