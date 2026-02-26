namespace DentiFlow.Application.DTOs;

public record ClinicaProfileDto(
    Guid Id,
    string Nombre,
    string Slug,
    string? LogoUrl,
    string? Telefono,
    string? Direccion,
    string? Descripcion,
    List<string> Especialidades,
    List<DentistaResumenDto> Dentistas);

public record DentistaResumenDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string? Especialidad);
