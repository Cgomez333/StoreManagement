# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StoreManagement.csproj", "."]
RUN dotnet restore "./StoreManagement.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "StoreManagement.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "StoreManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 1. Le decimos a ASP.NET Core que escuche en el puerto 8080 en cualquier dirección IP
ENV ASPNETCORE_URLS="http://+:${PORT:-8080}"

# 2. Exponemos el puerto 8080 para que Docker sepa que el contenedor escucha aquí
EXPOSE 8080

# Punto de entrada para ejecutar la aplicación
ENTRYPOINT ["dotnet", "StoreManagement.dll"]