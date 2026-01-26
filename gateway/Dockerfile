# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Fcg.Gateway/Fcg.Gateway.csproj Fcg.Gateway/
RUN dotnet restore "Fcg.Gateway/Fcg.Gateway.csproj"

COPY Fcg.Gateway/ Fcg.Gateway/
WORKDIR /src/Fcg.Gateway
RUN dotnet publish -c Release -o /app/publish --no-restore

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcg.Gateway.dll"]
