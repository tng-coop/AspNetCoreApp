# Base image with runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AspNetCoreApp.csproj", "./"]
RUN dotnet restore "AspNetCoreApp.csproj"
COPY . .
RUN dotnet publish "AspNetCoreApp.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AspNetCoreApp.dll"]
