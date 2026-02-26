FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY src/api/DentiFlow.Domain/DentiFlow.Domain.csproj DentiFlow.Domain/
COPY src/api/DentiFlow.Application/DentiFlow.Application.csproj DentiFlow.Application/
COPY src/api/DentiFlow.Infrastructure/DentiFlow.Infrastructure.csproj DentiFlow.Infrastructure/
COPY src/api/DentiFlow.API/DentiFlow.API.csproj DentiFlow.API/
RUN dotnet restore DentiFlow.API/DentiFlow.API.csproj

# Copy everything and build
COPY src/api/ .
RUN dotnet publish DentiFlow.API/DentiFlow.API.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DentiFlow.API.dll"]
