FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update && apt-get install -y curl iputils-ping net-tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src
COPY ["ChoreBuddies.Frontend/ChoreBuddies.Frontend.csproj", "ChoreBuddies.Frontend/"]
RUN dotnet restore "./ChoreBuddies.Frontend/ChoreBuddies.Frontend.csproj"
COPY . .
WORKDIR "/src/ChoreBuddies.Frontend"
RUN dotnet build "ChoreBuddies.Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "ChoreBuddies.Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChoreBuddies.Frontend.dll"]