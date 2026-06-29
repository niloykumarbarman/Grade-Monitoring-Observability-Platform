FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first (for layer caching)
COPY ["src/services/BuildingBlocks/Common/Common.csproj", "BuildingBlocks/Common/"]
COPY ["src/services/BuildingBlocks/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/"]
COPY ["src/services/OrderService/OrderService.API/OrderService.API.csproj", "OrderService/OrderService.API/"]
COPY ["src/services/OrderService/OrderService.Application/OrderService.Application.csproj", "OrderService/OrderService.Application/"]
COPY ["src/services/OrderService/OrderService.Domain/OrderService.Domain.csproj", "OrderService/OrderService.Domain/"]
COPY ["src/services/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService/OrderService.Infrastructure/"]
COPY ["src/services/BuildingBlocks/Observability/Observability.csproj", "BuildingBlocks/Observability/"]

RUN dotnet restore "OrderService/OrderService.API/OrderService.API.csproj"

# Copy full source
COPY src/services/BuildingBlocks/Common/ BuildingBlocks/Common/
COPY src/services/BuildingBlocks/EventBus/ BuildingBlocks/EventBus/
COPY src/services/BuildingBlocks/Observability/ BuildingBlocks/Observability/
COPY src/services/OrderService/ OrderService/

RUN dotnet build "OrderService/OrderService.API/OrderService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService/OrderService.API/OrderService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.API.dll"]
