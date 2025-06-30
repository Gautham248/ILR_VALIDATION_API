# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files for dependency restore
COPY ILR_VALIDATION.Api/ILR_VALIDATION.Api.csproj ILR_VALIDATION.Api/
COPY ILR_VALIDATION.Application/ILR_VALIDATION.Application.csproj ILR_VALIDATION.Application/
COPY ILR_VALIDATION.Domain/ILR_VALIDATION.Domain.csproj ILR_VALIDATION.Domain/
COPY ILR_VALIDATION.Infrastructure/ILR_VALIDATION.Infrastructure.csproj ILR_VALIDATION.Infrastructure/

# Restore dependencies
RUN dotnet restore ILR_VALIDATION.Api/ILR_VALIDATION.Api.csproj

# Copy all source files
COPY . .

# Build the app
WORKDIR /src/ILR_VALIDATION.Api
RUN dotnet build ILR_VALIDATION.Api.csproj -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish ILR_VALIDATION.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ILR_VALIDATION.Api.dll"]
