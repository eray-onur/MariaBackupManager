#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BackupManager.Worker/BackupManager.Worker.csproj", "BackupManager.Worker/"]
RUN dotnet restore "BackupManager.Worker/BackupManager.Worker.csproj"
COPY . .
WORKDIR "/src/BackupManager.Worker"
RUN dotnet build "BackupManager.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BackupManager.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false --self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackupManager.Worker.dll"]