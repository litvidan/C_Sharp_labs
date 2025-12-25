#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Install curl for health checks
RUN apt-get update && apt-get install -y curl
USER app
WORKDIR /app
EXPOSE 8090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DiningPhilosophers.CoordinatorService/DiningPhilosophers.CoordinatorService.csproj", "DiningPhilosophers.CoordinatorService/"]
COPY ["DiningPhilosophers.Contracts/DiningPhilosophers.Contracts.csproj", "DiningPhilosophers.Contracts/"]
RUN dotnet restore "./DiningPhilosophers.CoordinatorService/DiningPhilosophers.CoordinatorService.csproj"
COPY . .
WORKDIR "/src/DiningPhilosophers.CoordinatorService"
RUN dotnet build "./DiningPhilosophers.CoordinatorService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DiningPhilosophers.CoordinatorService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiningPhilosophers.CoordinatorService.dll"]
