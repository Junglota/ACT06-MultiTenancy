# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
# Cambia la ruta al csproj real:
RUN dotnet restore "./Primer parcial/Primer parcial.csproj"
RUN dotnet publish "./Primer parcial/Primer parcial.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway expone PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "Primer parcial.dll"]