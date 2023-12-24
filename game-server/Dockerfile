# Use the official Microsoft .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Set the .NET SDK as the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["game-server.csproj", "./"]
RUN dotnet restore "./game-server.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "game-server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "game-server.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "game-server.dll"]


#dotnet add package Microsoft.Extensions.DependencyInjection
#dotnet add package dotenv.net
#dotnet add package MessagePack