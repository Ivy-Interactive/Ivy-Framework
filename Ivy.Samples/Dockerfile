FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
WORKDIR /src
COPY ["Ivy.Samples/Ivy.Samples.csproj", "Ivy.Samples/"]
COPY ["Ivy.Samples.Shared/Ivy.Samples.Shared.csproj", "Ivy.Samples.Shared/"]
RUN dotnet restore "Ivy.Samples.Shared/Ivy.Samples.Shared.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION
RUN dotnet restore "Ivy.Samples/Ivy.Samples.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION
COPY . .
#WORKDIR "/src/Ivy.Samples"
#RUN dotnet build "Ivy.Samples.csproj" -c $BUILD_CONFIGURATION -o /app/build -p:IvyPackageVersion=$IVY_PACKAGE_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION
RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)
WORKDIR "/src/Ivy.Samples"
RUN dotnet publish "./Ivy.Samples.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:IvyPackageVersion=$IVY_PACKAGE_VERSION -v:diag

FROM base AS final
WORKDIR /app
ENV PORT=5010
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ivy.Samples.dll"]


