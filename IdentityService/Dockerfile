FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY IdentityService/IdentityService/IdentityService.csproj IdentityService/IdentityService/
RUN dotnet restore IdentityService/IdentityService/IdentityService.csproj

COPY IdentityService/ IdentityService/
RUN dotnet publish IdentityService/IdentityService/IdentityService.csproj \
    -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "IdentityService.dll"]
