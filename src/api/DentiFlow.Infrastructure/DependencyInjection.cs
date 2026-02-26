using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Data;
using DentiFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentiFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DentiFlowDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(DentiFlowDbContext).Assembly.FullName)));

        services.AddScoped<IClinicaRepository, ClinicaRepository>();
        services.AddScoped<IDentistaRepository, DentistaRepository>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<ICitaRepository, CitaRepository>();

        return services;
    }
}
