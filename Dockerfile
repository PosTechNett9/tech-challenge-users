ARG DOTNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release

# ---------- BUILD (SDK) ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build
WORKDIR /src

COPY FIAP.CloudGames.Users.sln ./
COPY src/FIAP.CloudGames.Users.API/FIAP.CloudGames.Users.API.csproj src/FIAP.CloudGames.Users.API/
COPY src/FIAP.CloudGames.Users.Application/FIAP.CloudGames.Users.Application.csproj src/FIAP.CloudGames.Users.Application/
COPY src/FIAP.CloudGames.Users.Domain/FIAP.CloudGames.Users.Domain.csproj src/FIAP.CloudGames.Users.Domain/
COPY src/FIAP.CloudGames.Users.Infrastructure/FIAP.CloudGames.Users.Infrastructure.csproj src/FIAP.CloudGames.Users.Infrastructure/

RUN dotnet restore src/FIAP.CloudGames.Users.API/FIAP.CloudGames.Users.API.csproj

COPY src/ ./src/

RUN dotnet publish src/FIAP.CloudGames.Users.API/FIAP.CloudGames.Users.API.csproj \
    -c ${BUILD_CONFIGURATION} \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# ---------- RUNTIME (ASP.NET) ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS="http://+:8080" \
    ASPNETCORE_ENVIRONMENT="Production" 

EXPOSE 8080

RUN addgroup -S app && adduser -S -G app -u 10001 app

COPY --from=build /app/publish/ ./

USER app

ENTRYPOINT ["dotnet", "FIAP.CloudGames.Users.API.dll"]
