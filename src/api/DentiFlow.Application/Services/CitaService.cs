using DentiFlow.Application.DTOs;
using DentiFlow.Domain.Entities;
using DentiFlow.Domain.Interfaces;

namespace DentiFlow.Application.Services;

public class CitaService
{
    private readonly ICitaRepository _citaRepo;
    private readonly IPacienteRepository _pacienteRepo;
    private readonly IDentistaRepository _dentistaRepo;

    public CitaService(ICitaRepository citaRepo, IPacienteRepository pacienteRepo, IDentistaRepository dentistaRepo)
    {
        _citaRepo = citaRepo;
        _pacienteRepo = pacienteRepo;
        _dentistaRepo = dentistaRepo;
    }

    public async Task<CitaDto> BookAppointmentAsync(CrearCitaRequest request, CancellationToken ct = default)
    {
        // Verificar que el dentista existe
        var dentista = await _dentistaRepo.GetByIdAsync(request.DentistaId, ct)
            ?? throw new InvalidOperationException("Dentista no encontrado.");

        // Verificar conflicto de horario
        var hayConflicto = await _citaRepo.ExisteConflictoAsync(
            request.DentistaId, request.FechaHora, request.DuracionMinutos, ct: ct);

        if (hayConflicto)
            throw new InvalidOperationException("El dentista ya tiene una cita en ese horario.");

        // Crear o buscar paciente
        var paciente = await _pacienteRepo.CreateAsync(new Paciente
        {
            ClinicaId = request.ClinicaId,
            Nombre = request.NombrePaciente,
            Apellido = request.ApellidoPaciente,
            Email = request.EmailPaciente,
            Telefono = request.TelefonoPaciente
        }, ct);

        // Crear la cita
        var cita = await _citaRepo.CreateAsync(new Cita
        {
            ClinicaId = request.ClinicaId,
            DentistaId = request.DentistaId,
            PacienteId = paciente.Id,
            FechaHora = request.FechaHora,
            DuracionMinutos = request.DuracionMinutos,
            Motivo = request.Motivo,
            Estado = EstadoCita.Pendiente
        }, ct);

        // Recargar con navegaciones
        var citaCompleta = await _citaRepo.GetByIdAsync(cita.Id, ct);
        return CitaMapper.ToDto(citaCompleta!);
    }

    public async Task<List<CitaDto>> GetByClinicaAsync(Guid clinicaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var citas = await _citaRepo.GetByClinicaAsync(clinicaId, desde, hasta, ct);
        return citas.Select(CitaMapper.ToDto).ToList();
    }

    public async Task<CitaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cita = await _citaRepo.GetByIdAsync(id, ct);
        return cita is null ? null : CitaMapper.ToDto(cita);
    }

    public async Task<CitaDto?> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var cita = await _citaRepo.GetByIdAsync(id, ct);
        if (cita is null) return null;

        cita.Estado = EstadoCita.Cancelada;
        await _citaRepo.UpdateAsync(cita, ct);

        return CitaMapper.ToDto(cita);
    }

    public async Task<CitaDto?> UpdateEstadoAsync(Guid id, EstadoCita nuevoEstado, CancellationToken ct = default)
    {
        var cita = await _citaRepo.GetByIdAsync(id, ct);
        if (cita is null) return null;

        cita.Estado = nuevoEstado;
        await _citaRepo.UpdateAsync(cita, ct);

        return CitaMapper.ToDto(cita);
    }
}
