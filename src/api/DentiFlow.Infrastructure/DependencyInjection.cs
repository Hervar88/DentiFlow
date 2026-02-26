using DentiFlow.Application.Interfaces;
using DentiFlow.Domain.Interfaces;
using DentiFlow.Infrastructure.Configuration;
using DentiFlow.Infrastructure.Data;
using DentiFlow.Infrastructure.ExternalServices;
using DentiFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentiFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        services.AddNpgsqlDataSource(connectionString, builder => builder.EnableDynamicJson());

        services.AddDbContext<DentiFlowDbContext>(options =>
            options.UseNpgsql(
                npgsql => npgsql.MigrationsAssembly(typeof(DentiFlowDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IClinicaRepository, ClinicaRepository>();
        services.AddScoped<IDentistaRepository, DentistaRepository>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<ICitaRepository, CitaRepository>();

        // Google Calendar
        services.Configure<GoogleCalendarOptions>(
            configuration.GetSection(GoogleCalendarOptions.SectionName));
        services.AddScoped<IGoogleCalendarService, GoogleCalendarServiceImpl>();

        // Mercado Pago
        services.Configure<MercadoPagoOptions>(
            configuration.GetSection(MercadoPagoOptions.SectionName));
        services.AddScoped<IMercadoPagoService, MercadoPagoServiceImpl>();

        return services;
    }
}
