FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Contracts/Contracts.csproj Contracts/
COPY NotificationServiceAPI/NotificationServiceAPI/NotificationServiceAPI.csproj NotificationServiceAPI/NotificationServiceAPI/
RUN dotnet restore NotificationServiceAPI/NotificationServiceAPI/NotificationServiceAPI.csproj

COPY Contracts/ Contracts/
COPY NotificationServiceAPI/ NotificationServiceAPI/
RUN dotnet publish NotificationServiceAPI/NotificationServiceAPI/NotificationServiceAPI.csproj \
    -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NotificationServiceAPI.dll"]
