FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
WORKDIR /src
COPY ["Ivy.Docs/Ivy.Docs.csproj", "Ivy.Docs/"]
RUN dotnet restore "Ivy.Docs/Ivy.Docs.csproj" -p:UseNuGetPackage=true -p:IvyPackageVersion=$IVY_PACKAGE_VERSION
COPY . .
WORKDIR "/src/Ivy.Docs"
RUN dotnet build "./Ivy.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/build -p:UseNuGetPackage=true -p:IvyPackageVersion=$IVY_PACKAGE_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
RUN dotnet publish "./Ivy.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -p:UseNuGetPackage=true -p:IvyPackageVersion=$IVY_PACKAGE_VERSION

FROM base AS final
WORKDIR /app
ENV PORT=5010
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ivy.Docs.dll"]


