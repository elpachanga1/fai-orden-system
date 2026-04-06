# ---------------------------------------------------------------
# Multi-stage build para .NET 8 API
#
# CONTEXTO: el build context es backend-dotnet/ (raiz de la solucion).
# Esto permite que el SDK acceda a todos los proyectos referenciados
# (BusinessRules, DataRepository, Services, Validations).
#
# USO desde CI (az acr build):
#   az acr build \
#     --registry <acr-name> \
#     --image backend:<tag> \
#     --file src/CarritoComprasBackend/Dockerfile \
#     .   <- contexto = backend-dotnet/
# ---------------------------------------------------------------

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero para aprovechar cache de capas
COPY src/CarritoComprasBackend/ShoppingCartBackEnd.csproj           src/CarritoComprasBackend/
COPY src/BusinessRules/BusinessRules.csproj                         src/BusinessRules/
COPY src/DataRepository/DataRepository.csproj                       src/DataRepository/
COPY src/Services/Services.csproj                                   src/Services/
COPY src/Validations/Validations.csproj                             src/Validations/

RUN dotnet restore src/CarritoComprasBackend/ShoppingCartBackEnd.csproj

# Copiar todo el codigo fuente
COPY src/ src/

RUN dotnet publish src/CarritoComprasBackend/ShoppingCartBackEnd.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /consoleloggerparameters:NoSummary

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Puerto 8080: Azure Container Apps enruta el trafico a este puerto por convencion.
# ASPNETCORE_HTTP_PORTS reemplaza a ASPNETCORE_URLS en .NET 8+.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ShoppingCartBackEnd.dll"]
