# Zobacz https://aka.ms/customizecontainer, aby dowiedzieæ siê, jak dostosowaæ kontener debugowania i jak program Visual Studio u¿ywa tego pliku Dockerfile do kompilowania obrazów w celu szybszego debugowania.

# Ten etap jest u¿ywany podczas uruchamiania z programu VS w trybie szybkim (domyœlnie dla konfiguracji debugowania)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y \
    curl \
    iputils-ping \
    net-tools \
 && rm -rf /var/lib/apt/lists/*


# Ten etap s³u¿y do kompilowania projektu us³ugi
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src
COPY ["ChoreBuddies.Backend/ChoreBuddies.Backend.csproj", "ChoreBuddies.Backend/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "ChoreBuddies.Backend/ChoreBuddies.Backend.csproj"
COPY . .
WORKDIR "/src/ChoreBuddies.Backend"
RUN dotnet build "ChoreBuddies.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Ten etap s³u¿y do publikowania projektu us³ugi do skopiowania do etapu koñcowego
FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "ChoreBuddies.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Ten etap jest u¿ywany w œrodowisku produkcyjnym lub w przypadku uruchamiania z programu VS w trybie regularnym (domyœlnie, gdy nie jest u¿ywana konfiguracja debugowania)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChoreBuddies.Backend.dll"]