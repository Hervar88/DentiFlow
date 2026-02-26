using DentiFlow.Application.DTOs;
using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;

namespace DentiFlow.Application.Services;

public class PacienteService
{
    private readonly IPacienteRepository _repo;

    public PacienteService(IPacienteRepository repo) => _repo = repo;

    public async Task<List<PacienteDto>> GetByClinicaAsync(Guid clinicaId, CancellationToken ct = default)
    {
        var pacientes = await _repo.GetByClinicaAsync(clinicaId, ct);
        return pacientes.Select(p => new PacienteDto(
            p.Id, p.ClinicaId, p.Nombre, p.Apellido, p.Email, p.Telefono, p.Notas, p.CreatedAt
        )).ToList();
    }

    public async Task<PacienteDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        if (p is null) return null;
        return new PacienteDto(p.Id, p.ClinicaId, p.Nombre, p.Apellido, p.Email, p.Telefono, p.Notas, p.CreatedAt);
    }

    public async Task<PacienteDto> CreateAsync(CrearPacienteRequest request, CancellationToken ct = default)
    {
        var paciente = await _repo.CreateAsync(new Paciente
        {
            ClinicaId = request.ClinicaId,
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            Telefono = request.Telefono,
            Notas = request.Notas,
        }, ct);

        return new PacienteDto(
            paciente.Id, paciente.ClinicaId, paciente.Nombre, paciente.Apellido,
            paciente.Email, paciente.Telefono, paciente.Notas, paciente.CreatedAt);
    }

    public async Task<PacienteDto?> UpdateAsync(Guid id, ActualizarPacienteRequest request, CancellationToken ct = default)
    {
        var paciente = await _repo.GetByIdAsync(id, ct);
        if (paciente is null) return null;

        paciente.Nombre = request.Nombre;
        paciente.Apellido = request.Apellido;
        paciente.Email = request.Email;
        paciente.Telefono = request.Telefono;
        paciente.Notas = request.Notas;

        await _repo.UpdateAsync(paciente, ct);

        return new PacienteDto(
            paciente.Id, paciente.ClinicaId, paciente.Nombre, paciente.Apellido,
            paciente.Email, paciente.Telefono, paciente.Notas, paciente.CreatedAt);
    }
}
