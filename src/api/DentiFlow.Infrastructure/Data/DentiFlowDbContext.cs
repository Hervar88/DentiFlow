using DentiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentiFlow.Infrastructure.Data;

public class DentiFlowDbContext : DbContext
{
    public DentiFlowDbContext(DbContextOptions<DentiFlowDbContext> options) : base(options) { }

    public DbSet<Clinica> Clinicas => Set<Clinica>();
    public DbSet<Dentista> Dentistas => Set<Dentista>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Cita> Citas => Set<Cita>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Clinica
        modelBuilder.Entity<Clinica>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nombre).HasMaxLength(200).IsRequired();
            e.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.Especialidades).HasColumnType("jsonb");
        });

        // Dentista
        modelBuilder.Entity<Dentista>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Nombre).HasMaxLength(100).IsRequired();
            e.Property(d => d.Apellido).HasMaxLength(100).IsRequired();
            e.Property(d => d.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(d => d.Email).IsUnique();
            e.HasOne(d => d.Clinica)
                .WithMany(c => c.Dentistas)
                .HasForeignKey(d => d.ClinicaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Paciente
        modelBuilder.Entity<Paciente>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            e.Property(p => p.Apellido).HasMaxLength(100).IsRequired();
            e.HasOne(p => p.Clinica)
                .WithMany(c => c.Pacientes)
                .HasForeignKey(p => p.ClinicaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Cita
        modelBuilder.Entity<Cita>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
            e.HasOne(c => c.Clinica)
                .WithMany()
                .HasForeignKey(c => c.ClinicaId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Dentista)
                .WithMany(d => d.Citas)
                .HasForeignKey(c => c.DentistaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(c => new { c.DentistaId, c.FechaHora });
        });
    }
}
