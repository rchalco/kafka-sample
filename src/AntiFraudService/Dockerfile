FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY Listener.AntiFraud.Core/Listener.AntiFraud.Core.csproj ./Listener.AntiFraud.Core/
COPY Listener.AntiFraud.Infrastructure/Listener.AntiFraud.Infrastructure.csproj ./Listener.AntiFraud.Infrastructure/
COPY Listener.AntiFraud/Listener.AntiFraud.csproj ./Listener.AntiFraud/
RUN dotnet restore ./Listener.AntiFraud/Listener.AntiFraud.csproj

# Copy all source and publish
COPY . .
RUN dotnet publish ./Listener.AntiFraud/Listener.AntiFraud.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Listener.AntiFraud.dll"]
