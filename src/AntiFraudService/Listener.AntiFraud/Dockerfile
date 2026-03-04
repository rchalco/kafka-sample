FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Listener.AntiFraud.csproj ./
RUN dotnet restore Listener.AntiFraud.csproj
COPY . .
RUN dotnet publish Listener.AntiFraud.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Listener.AntiFraud.dll"]
