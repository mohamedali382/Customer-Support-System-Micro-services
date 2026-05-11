FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG SERVICE_NAME
ARG SERVICE_PATH

WORKDIR /source

COPY . .

RUN dotnet publish ${SERVICE_PATH}/${SERVICE_NAME}.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

ARG SERVICE_NAME
ENV SERVICE_DLL=${SERVICE_NAME}.dll

WORKDIR  /app
COPY --from=build /app .

EXPOSE 5041

ENTRYPOINT ["sh", "-c", "dotnet ${SERVICE_DLL}"]