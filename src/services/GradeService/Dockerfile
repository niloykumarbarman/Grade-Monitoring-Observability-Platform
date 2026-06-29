FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first (for layer caching)
COPY ["src/services/BuildingBlocks/Common/Common.csproj", "BuildingBlocks/Common/"]
COPY ["src/services/BuildingBlocks/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/"]
COPY ["src/services/BuildingBlocks/Observability/Observability.csproj", "BuildingBlocks/Observability/"]
COPY ["src/services/GradeService/src/GradeService.API/GradeService.API.csproj", "GradeService/src/GradeService.API/"]
COPY ["src/services/GradeService/src/GradeService.Application/GradeService.Application.csproj", "GradeService/src/GradeService.Application/"]
COPY ["src/services/GradeService/src/GradeService.Domain/GradeService.Domain.csproj", "GradeService/src/GradeService.Domain/"]
COPY ["src/services/GradeService/src/GradeService.Infrastructure/GradeService.Infrastructure.csproj", "GradeService/src/GradeService.Infrastructure/"]

RUN dotnet restore "GradeService/src/GradeService.API/GradeService.API.csproj"

# Copy full source
COPY src/services/BuildingBlocks/Common/ BuildingBlocks/Common/
COPY src/services/BuildingBlocks/EventBus/ BuildingBlocks/EventBus/
COPY src/services/BuildingBlocks/Observability/ BuildingBlocks/Observability/
COPY src/services/GradeService/ GradeService/

RUN dotnet build "GradeService/src/GradeService.API/GradeService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GradeService/src/GradeService.API/GradeService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GradeService.API.dll"]
