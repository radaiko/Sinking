# Use the official .NET 9.0 runtime as a parent image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Sinking.Api/Sinking.Api.csproj", "src/Sinking.Api/"]
COPY ["src/Sinking.Console/Sinking.Console.csproj", "src/Sinking.Console/"]
COPY ["src/Sinking.Core/Sinking.Core.csproj", "src/Sinking.Core/"]
COPY ["src/Sinking.Data/Sinking.Data.csproj", "src/Sinking.Data/"]
RUN dotnet restore "src/Sinking.Api/Sinking.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Sinking.Api"
RUN dotnet build "Sinking.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sinking.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sinking.Api.dll"]