FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/TaskManager.Auth/TaskManager.Auth.csproj", "src/TaskManager.Auth/"]
COPY ["src/TaskManager.Application/TaskManager.Application.csproj", "src/TaskManager.Application/"]
COPY ["src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "src/TaskManager.Infrastructure/"]
COPY ["src/TaskManager.Domain/TaskManager.Domain.csproj", "src/TaskManager.Domain/"]
RUN dotnet restore "src/TaskManager.Auth/TaskManager.Auth.csproj"
COPY . .
RUN dotnet build "src/TaskManager.Auth/TaskManager.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TaskManager.Auth/TaskManager.Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Auth.dll"]
