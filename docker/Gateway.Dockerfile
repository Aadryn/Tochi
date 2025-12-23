FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["backend/src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj", "Presentation/LLMProxy.Gateway/"]
COPY ["backend/src/Core/LLMProxy.Domain/LLMProxy.Domain.csproj", "Core/LLMProxy.Domain/"]
COPY ["backend/src/Application/LLMProxy.Application/LLMProxy.Application.csproj", "Application/LLMProxy.Application/"]
COPY ["backend/src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/LLMProxy.Infrastructure.PostgreSQL.csproj", "Infrastructure/LLMProxy.Infrastructure.PostgreSQL/"]
COPY ["backend/src/Infrastructure/LLMProxy.Infrastructure.Redis/LLMProxy.Infrastructure.Redis.csproj", "Infrastructure/LLMProxy.Infrastructure.Redis/"]
COPY ["backend/src/Infrastructure/LLMProxy.Infrastructure.Security/LLMProxy.Infrastructure.Security.csproj", "Infrastructure/LLMProxy.Infrastructure.Security/"]
COPY ["backend/src/Infrastructure/LLMProxy.Infrastructure.Security.Abstractions/LLMProxy.Infrastructure.Security.Abstractions.csproj", "Infrastructure/LLMProxy.Infrastructure.Security.Abstractions/"]
COPY ["backend/src/Infrastructure/LLMProxy.Infrastructure.Telemetry/LLMProxy.Infrastructure.Telemetry.csproj", "Infrastructure/LLMProxy.Infrastructure.Telemetry/"]
COPY ["backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/LLMProxy.Infrastructure.LLMProviders.csproj", "Infrastructure/LLMProxy.Infrastructure.LLMProviders/"]
COPY ["backend/src/Infrastructure/LLMProxy.Infrastructure.Configuration/LLMProxy.Infrastructure.Configuration.csproj", "Infrastructure/LLMProxy.Infrastructure.Configuration/"]

RUN dotnet restore "Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj"

# Copy backend source and build
COPY backend/src/ .
WORKDIR "/src/Presentation/LLMProxy.Gateway"
RUN dotnet build "LLMProxy.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LLMProxy.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LLMProxy.Gateway.dll"]
