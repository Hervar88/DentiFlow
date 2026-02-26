using DentiFlow.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DentiFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ClinicaService>();
        services.AddScoped<CitaService>();
        services.AddScoped<DentistaService>();
        services.AddScoped<PacienteService>();
        return services;
    }
}
