# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ChoreBuddies.Frontend/ChoreBuddies.Frontend.csproj", "ChoreBuddies.Frontend/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "ChoreBuddies.Frontend/ChoreBuddies.Frontend.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ChoreBuddies.Frontend"
RUN dotnet publish -c Release -o /app/publish

# Runtime stage - use a lightweight web server
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static files
RUN rm -rf ./*

# Copy published Blazor WASM files
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy nginx configuration (optional but recommended for Blazor WASM)
COPY Docker/nginx.conf /etc/nginx/nginx.conf

# Expose port 8080
EXPOSE 8080

# Nginx will start automatically with the base image