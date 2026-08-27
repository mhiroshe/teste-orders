FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OrderManagement.sln .
COPY src/OrderManagement.Domain/OrderManagement.Domain.csproj             src/OrderManagement.Domain/
COPY src/OrderManagement.Application/OrderManagement.Application.csproj   src/OrderManagement.Application/
COPY src/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj src/OrderManagement.Infrastructure/
COPY src/OrderManagement.API/OrderManagement.API.csproj                   src/OrderManagement.API/
COPY tests/OrderManagement.UnitTests/OrderManagement.UnitTests.csproj     tests/OrderManagement.UnitTests/
COPY tests/OrderManagement.IntegrationTests/OrderManagement.IntegrationTests.csproj tests/OrderManagement.IntegrationTests/

RUN dotnet restore

COPY . .

RUN dotnet test --no-restore --configuration Release

RUN dotnet publish src/OrderManagement.API/OrderManagement.API.csproj \
    --no-restore \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/data

COPY --from=build /app/publish .

ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/orders.db"
ENV ASPNETCORE_URLS="http://+:8080"

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderManagement.API.dll"]
