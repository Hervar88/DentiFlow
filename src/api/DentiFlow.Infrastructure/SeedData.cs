using DentiFlow.Domain.Entities;
using DentiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DentiFlow.Infrastructure;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DentiFlowDbContext>();

        if (await db.Clinicas.AnyAsync()) return;

        var clinicaId = Guid.NewGuid();
        var dentistaId = Guid.NewGuid();

        var clinica = new Clinica
        {
            Id = clinicaId,
            Nombre = "Dental Sonrisa MX",
            Slug = "dental-sonrisa-mx",
            Telefono = "+52 55 1234 5678",
            Direccion = "Av. Reforma 123, Col. Centro, CDMX",
            Descripcion = "Clínica dental especializada en ortodoncia, implantes y estética dental con más de 10 años de experiencia.",
            LogoUrl = null,
            Especialidades = ["Ortodoncia", "Implantes", "Endodoncia", "Estética Dental", "Limpieza"],
        };

        var dentista = new Dentista
        {
            Id = dentistaId,
            ClinicaId = clinicaId,
            Nombre = "Carlos",
            Apellido = "Ramírez",
            Email = "carlos@dentalsonrisa.mx",
            Especialidad = "Ortodoncia",
            Telefono = "+52 55 9876 5432",
        };

        var dentista2 = new Dentista
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            Nombre = "María",
            Apellido = "González",
            Email = "maria@dentalsonrisa.mx",
            Especialidad = "Endodoncia",
            Telefono = "+52 55 5555 1234",
        };

        var paciente1 = new Paciente
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan.perez@email.com",
            Telefono = "+52 55 1111 2222",
            Notas = "Alérgico a la penicilina",
        };

        var paciente2 = new Paciente
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            Nombre = "Ana",
            Apellido = "López",
            Email = "ana.lopez@email.com",
            Telefono = "+52 55 3333 4444",
        };

        db.Clinicas.Add(clinica);
        db.Dentistas.AddRange(dentista, dentista2);
        db.Pacientes.AddRange(paciente1, paciente2);

        // Citas de ejemplo (próximos días)
        var hoy = DateTime.UtcNow.Date.AddHours(9);
        db.Citas.AddRange(
            new Cita
            {
                Id = Guid.NewGuid(),
                ClinicaId = clinicaId,
                DentistaId = dentistaId,
                PacienteId = paciente1.Id,
                FechaHora = hoy.AddDays(1),
                DuracionMinutos = 30,
                Motivo = "Revisión general",
                Estado = EstadoCita.Confirmada,
            },
            new Cita
            {
                Id = Guid.NewGuid(),
                ClinicaId = clinicaId,
                DentistaId = dentistaId,
                PacienteId = paciente2.Id,
                FechaHora = hoy.AddDays(1).AddHours(1),
                DuracionMinutos = 60,
                Motivo = "Limpieza dental",
                Estado = EstadoCita.Pendiente,
            },
            new Cita
            {
                Id = Guid.NewGuid(),
                ClinicaId = clinicaId,
                DentistaId = dentista2.Id,
                PacienteId = paciente1.Id,
                FechaHora = hoy.AddDays(2).AddHours(2),
                DuracionMinutos = 45,
                Motivo = "Endodoncia molar #36",
                Estado = EstadoCita.Pendiente,
            }
        );

        await db.SaveChangesAsync();
    }
}
