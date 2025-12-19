#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DiningPhilosophers.PhilosopherService/DiningPhilosophers.PhilosopherService.csproj", "DiningPhilosophers.PhilosopherService/"]
COPY ["DiningPhilosophers.Contracts/DiningPhilosophers.Contracts.csproj", "DiningPhilosophers.Contracts/"]
RUN dotnet restore "./DiningPhilosophers.PhilosopherService/DiningPhilosophers.PhilosopherService.csproj"
COPY . .
WORKDIR "/src/DiningPhilosophers.PhilosopherService"
RUN dotnet build "./DiningPhilosophers.PhilosopherService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DiningPhilosophers.PhilosopherService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiningPhilosophers.PhilosopherService.dll"]
