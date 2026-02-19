# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /src

COPY Fcg.Gateway/Fcg.Gateway.csproj Fcg.Gateway/
RUN dotnet restore Fcg.Gateway/Fcg.Gateway.csproj

COPY Fcg.Gateway/ Fcg.Gateway/
RUN dotnet publish Fcg.Gateway/Fcg.Gateway.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root
RUN groupadd -r appgroup && useradd -r -g appgroup appuser
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build --chown=appuser:appgroup /app/publish .
ENTRYPOINT ["dotnet", "Fcg.Gateway.dll"]
