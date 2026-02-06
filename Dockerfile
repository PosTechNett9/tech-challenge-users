ARG DOTNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build
ARG DOTNET_VERSION
ARG BUILD_CONFIGURATION
WORKDIR /src

COPY FIAP.CloudGames.Users.sln ./
COPY src/FIAP.CloudGames.Users.API/FIAP.CloudGames.Users.API.csproj src/FIAP.CloudGames.Users.API/
COPY src/FIAP.CloudGames.Users.Application/FIAP.CloudGames.Users.Application.csproj src/FIAP.CloudGames.Users.Application/
COPY src/FIAP.CloudGames.Users.Domain/FIAP.CloudGames.Users.Domain.csproj src/FIAP.CloudGames.Users.Domain/
COPY src/FIAP.CloudGames.Users.Infrastructure/FIAP.CloudGames.Users.Infrastructure.csproj src/FIAP.CloudGames.Users.Infrastructure/

RUN dotnet restore src/FIAP.CloudGames.Users.API/FIAP.CloudGames.Users.API.csproj
COPY src/ ./src/

RUN dotnet publish src/FIAP.CloudGames.Users.API/FIAP.CloudGames.Users.API.csproj \
    -c ${BUILD_CONFIGURATION} -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS="http://+:8080" \
    ASPNETCORE_ENVIRONMENT="Production" \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

COPY --from=build --chown=app:app /app/publish/ ./

USER app
ENTRYPOINT ["dotnet", "FIAP.CloudGames.Users.API.dll"]