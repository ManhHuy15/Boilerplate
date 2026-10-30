# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copy solution and project manifests first for layer caching
COPY Boilerplate.slnx Directory.Build.props global.json ./
COPY Boilerplate.Domain/Boilerplate.Domain.csproj Boilerplate.Domain/
COPY Boilerplate.Application/Boilerplate.Application.csproj Boilerplate.Application/
COPY Boilerplate.Infrastructure/Boilerplate.Infrastructure.csproj Boilerplate.Infrastructure/
COPY Boilerplate.API/Boilerplate.API.csproj Boilerplate.API/

RUN dotnet restore Boilerplate.slnx

# Copy source code and build Release
COPY . .

RUN dotnet publish Boilerplate.API/Boilerplate.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage (minimal chiseled image, non-root)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["dotnet", "Boilerplate.API.dll"]
