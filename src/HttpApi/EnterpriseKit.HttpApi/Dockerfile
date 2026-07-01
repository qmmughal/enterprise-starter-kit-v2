FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for layer caching
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Domain/EnterpriseKit.Domain/EnterpriseKit.Domain.csproj", "src/Domain/EnterpriseKit.Domain/"]
COPY ["src/Application/EnterpriseKit.Application/EnterpriseKit.Application.csproj", "src/Application/EnterpriseKit.Application/"]
COPY ["src/Infrastructure/EnterpriseKit.Infrastructure/EnterpriseKit.Infrastructure.csproj", "src/Infrastructure/EnterpriseKit.Infrastructure/"]
COPY ["src/HttpApi/EnterpriseKit.HttpApi/EnterpriseKit.HttpApi.csproj", "src/HttpApi/EnterpriseKit.HttpApi/"]

RUN dotnet restore "src/HttpApi/EnterpriseKit.HttpApi/EnterpriseKit.HttpApi.csproj"

COPY . .

WORKDIR "/src/src/HttpApi/EnterpriseKit.HttpApi"
RUN dotnet build "EnterpriseKit.HttpApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EnterpriseKit.HttpApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup --shell /bin/sh appuser

COPY --from=publish --chown=appuser:appgroup /app/publish .

USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "EnterpriseKit.HttpApi.dll"]
