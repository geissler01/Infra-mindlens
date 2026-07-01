FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore as distinct layers
COPY ["backend/MindLens.Api.csproj", "backend/"]
COPY ["migrator/MindLens.Migrator.csproj", "migrator/"]
RUN dotnet restore "migrator/MindLens.Migrator.csproj"

# Copy the rest of the code
COPY backend/ backend/
COPY migrator/ migrator/

WORKDIR "/src/migrator"
RUN dotnet publish "MindLens.Migrator.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Install necessary tools for Npgsql / postgres (if any, though not strictly needed for EF Core)
RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "MindLens.Migrator.dll"]
