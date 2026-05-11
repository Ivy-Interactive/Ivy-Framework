FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS toolchain
WORKDIR /src
RUN for i in 1 2 3; do apt-get update && break || sleep 10; done && \
    apt-get install -y --fix-missing curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    for i in 1 2 3; do apt-get update && break || sleep 10; done && \
    apt-get install -y --fix-missing nodejs && \
    npm install -g pnpm@10.33.0 vite-plus@0.1.20 && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

FROM toolchain AS build
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
WORKDIR /src
COPY ["src/Ivy.Samples/Ivy.Samples.csproj", "src/Ivy.Samples/"]
COPY ["src/Ivy.Samples.Shared/Ivy.Samples.Shared.csproj", "src/Ivy.Samples.Shared/"]
RUN dotnet restore "src/Ivy.Samples.Shared/Ivy.Samples.Shared.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION
RUN dotnet restore "src/Ivy.Samples/Ivy.Samples.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION
COPY . .
#WORKDIR "/src/Ivy.Samples"
#RUN dotnet build "Ivy.Samples.csproj" -c $BUILD_CONFIGURATION -o /app/build -p:IvyPackageVersion=$IVY_PACKAGE_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
WORKDIR "/src/src/Ivy.Samples"
RUN dotnet publish "./Ivy.Samples.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:IvyPackageVersion=$IVY_PACKAGE_VERSION -v:diag

FROM base AS final
WORKDIR /app
ENV PORT=5010
USER root
RUN for i in 1 2 3; do apt-get update && break || sleep 10; done && \
    apt-get install -y --no-install-recommends --fix-missing \
    libc6 \
    libgcc-s1 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ivy.Samples.dll"]
