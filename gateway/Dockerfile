# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 🔥 Mata qualquer config herdada
ENV NUGET_PACKAGES=/root/.nuget/packages
ENV NUGET_FALLBACK_PACKAGES=""
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /src

# Copia só o csproj primeiro (cache)
COPY Fcg.Gateway/Fcg.Gateway.csproj Fcg.Gateway/

# 🔥 restore LIMPO, sem fallback
RUN dotnet restore "Fcg.Gateway/Fcg.Gateway.csproj" \
    --no-cache \
    --disable-parallel

# Copia o resto do código
COPY Fcg.Gateway/ Fcg.Gateway/

WORKDIR /src/Fcg.Gateway

RUN dotnet publish -c Release -o /app/publish --no-restore
