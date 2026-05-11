FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Contracts/Contracts.csproj Contracts/
COPY TicketService/TicketService/TicketService.csproj TicketService/TicketService/
RUN dotnet restore TicketService/TicketService/TicketService.csproj

COPY Contracts/ Contracts/
COPY TicketService/ TicketService/
RUN dotnet publish TicketService/TicketService/TicketService.csproj \
    -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "TicketService.dll"]
