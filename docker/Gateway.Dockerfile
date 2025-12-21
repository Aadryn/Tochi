FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj", "src/Presentation/LLMProxy.Gateway/"]
COPY ["src/Core/LLMProxy.Domain/LLMProxy.Domain.csproj", "src/Core/LLMProxy.Domain/"]
COPY ["src/Application/LLMProxy.Application/LLMProxy.Application.csproj", "src/Application/LLMProxy.Application/"]
COPY ["src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/LLMProxy.Infrastructure.PostgreSQL.csproj", "src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/"]
COPY ["src/Infrastructure/LLMProxy.Infrastructure.Redis/LLMProxy.Infrastructure.Redis.csproj", "src/Infrastructure/LLMProxy.Infrastructure.Redis/"]
COPY ["src/Infrastructure/LLMProxy.Infrastructure.Security/LLMProxy.Infrastructure.Security.csproj", "src/Infrastructure/LLMProxy.Infrastructure.Security/"]
COPY ["src/Infrastructure/LLMProxy.Infrastructure.Telemetry/LLMProxy.Infrastructure.Telemetry.csproj", "src/Infrastructure/LLMProxy.Infrastructure.Telemetry/"]
COPY ["src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/LLMProxy.Infrastructure.LLMProviders.csproj", "src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/"]

RUN dotnet restore "src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Presentation/LLMProxy.Gateway"
RUN dotnet build "LLMProxy.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LLMProxy.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LLMProxy.Gateway.dll"]
