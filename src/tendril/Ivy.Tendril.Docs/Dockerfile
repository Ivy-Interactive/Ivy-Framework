FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION

WORKDIR /src

# Copy tool config and restore tools
# Note: Use the Ivy-Framework root as build context
COPY [".config/dotnet-tools.json", ".config/"]
RUN dotnet tool restore

# Copy projects for restoration
COPY ["src/tendril/Ivy.Tendril.Docs/Ivy.Tendril.Docs.csproj", "tendril/Ivy.Tendril.Docs/"]
COPY ["src/Ivy/Ivy.csproj", "Ivy/"]
COPY ["src/Ivy.Docs.Helpers/Ivy.Docs.Helpers.csproj", "Ivy.Docs.Helpers/"]
COPY ["src/Ivy.Analyser/Ivy.Analyser.csproj", "Ivy.Analyser/"]
COPY ["src/Ivy.Agent.Filter/Ivy.Agent.Filter.csproj", "Ivy.Agent.Filter/"]

# Restore
RUN dotnet restore "tendril/Ivy.Tendril.Docs/Ivy.Tendril.Docs.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish
WORKDIR "/src/tendril/Ivy.Tendril.Docs"
RUN dotnet publish "Ivy.Tendril.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libc6 \
    libgcc-s1 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ivy.Tendril.Docs.dll"]
