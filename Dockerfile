FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FinancePlanner.API/FinancePlanner.API.csproj FinancePlanner.API/
COPY FinancePlanner.Infrastructure/FinancePlanner.Infrastructure.csproj FinancePlanner.Infrastructure/
COPY FinancePlanner.Common/FinancePlanner.Common.csproj FinancePlanner.Common/
COPY FinancePlanner.UnitTests/FinancePlanner.UnitTests.csproj FinancePlanner.UnitTests/

RUN dotnet restore FinancePlanner.API/FinancePlanner.API.csproj

COPY . .

RUN dotnet publish FinancePlanner.API/FinancePlanner.API.csproj \
  --configuration Release \
  --output /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinancePlanner.API.dll"]
