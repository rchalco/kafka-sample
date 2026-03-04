FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY Api.Transaction.Core/Api.Transaction.Core.csproj ./Api.Transaction.Core/
COPY Api.Transaction.Infrastructure/Api.Transaction.Infrastructure.csproj ./Api.Transaction.Infrastructure/
COPY Api.Transaction/Api.Transaction.csproj ./Api.Transaction/
RUN dotnet restore ./Api.Transaction/Api.Transaction.csproj

# Copy all source and publish
COPY . .
RUN dotnet publish ./Api.Transaction/Api.Transaction.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.Transaction.dll"]
