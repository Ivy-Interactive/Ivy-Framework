FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS toolchain
WORKDIR /src
RUN apt-get update && apt-get install -y curl gnupg build-essential && \
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs && \
    curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | bash -s -- -y && \
    npm install -g pnpm@10.33.0 vite-plus@0.1.14 && \
    apt-get clean && rm -rf /var/lib/apt/lists/*
ENV PATH="/root/.cargo/bin:${PATH}"

FROM toolchain AS build
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
WORKDIR /src
COPY ["src/Ivy.Docs/Ivy.Docs.csproj", "src/Ivy.Docs/"]
COPY ["src/Ivy.Docs.Shared/Ivy.Docs.Shared.csproj", "src/Ivy.Docs.Shared/"]
RUN dotnet restore "src/Ivy.Docs.Shared/Ivy.Docs.Shared.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION
RUN dotnet restore "src/Ivy.Docs/Ivy.Docs.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION
COPY . .
#WORKDIR "/src/Ivy.Docs"
#RUN dotnet build "Ivy.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/build -p:IvyPackageVersion=$IVY_PACKAGE_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
WORKDIR "/src/src/Ivy.Docs"
RUN dotnet publish "./Ivy.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:IvyPackageVersion=$IVY_PACKAGE_VERSION -v:diag

FROM base AS final
WORKDIR /app
ENV PORT=5010
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libc6 \
    libgcc-s1 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ivy.Docs.dll"]


