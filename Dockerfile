# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
# Cambia la ruta al csproj real:
RUN dotnet restore "./ACT06 MultiTenancy/ACT06 MultiTenancy.csproj"
RUN dotnet publish "./ACT06 MultiTenancy/ACT06 MultiTenancy.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway expone PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "ACT06 MultiTenancy.dll"]