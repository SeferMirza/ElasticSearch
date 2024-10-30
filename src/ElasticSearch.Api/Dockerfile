FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["./ElasticSearch.Api.csproj", "./"]
RUN dotnet restore "ElasticSearch.Api.csproj"

COPY ./ ./
RUN dotnet publish "ElasticSearch.Api.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app ./
ENTRYPOINT [ "dotnet", "ElasticSearch.Api.dll" ]