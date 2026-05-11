FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Contracts/Contracts.csproj Contracts/
COPY SupportService/SupportService/SupportService.csproj SupportService/SupportService/
RUN dotnet restore SupportService/SupportService/SupportService.csproj

COPY Contracts/ Contracts/
COPY SupportService/ SupportService/
RUN dotnet publish SupportService/SupportService/SupportService.csproj \
    -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "SupportService.dll"]
