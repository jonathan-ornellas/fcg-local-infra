# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

WORKDIR /src

# Copia TODO o projeto de uma vez
# (evita problemas de restore/publish em passos separados)
COPY Fcg.Gateway/ Fcg.Gateway/

WORKDIR /src/Fcg.Gateway

# 🔥 UM comando só:
# restore + build + publish no mesmo contexto
RUN dotnet publish \
    -c Release \
    -o /app/publish \
    /p:RestoreFallbackFolders= \
    /p:RestoreIgnoreFailedSources=true \
    /p:RestoreNoCache=true

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Fcg.Gateway.dll"]
