# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 🔥 força NuGet a usar pasta Linux
ENV NUGET_PACKAGES=/root/.nuget/packages

WORKDIR /src

COPY Fcg.Gateway/Fcg.Gateway.csproj Fcg.Gateway/
RUN dotnet restore "Fcg.Gateway/Fcg.Gateway.csproj"

COPY Fcg.Gateway/ Fcg.Gateway/
WORKDIR /src/Fcg.Gateway
RUN dotnet publish -c Release -o /app/publish --no-restore
