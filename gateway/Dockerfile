# =========================
# 🔹 BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["gateway/Fcg.Gateway/Fcg.Gateway.csproj", "gateway/Fcg.Gateway/"]
RUN dotnet restore "gateway/Fcg.Gateway/Fcg.Gateway.csproj"

COPY gateway/Fcg.Gateway/ gateway/Fcg.Gateway/
WORKDIR /src/gateway/Fcg.Gateway
RUN dotnet publish -c Release -o /app/publish --no-restore

# =========================
# 🔹 RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Fcg.Gateway.dll"]
