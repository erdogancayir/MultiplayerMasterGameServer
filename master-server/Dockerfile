# Use the official Microsoft .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Set the .NET SDK as the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["master-server.csproj", "./"]
RUN dotnet restore "./master-server.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "master-server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "master-server.csproj" -c Release -o /app/publish

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "master-server.dll"]

#dotnet add package NUnit
#dotnet add package NUnit3TestAdapter
#dotnet add package Moq
#Newtonsoft.Json