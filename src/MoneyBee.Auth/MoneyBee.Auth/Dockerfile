FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MoneyBee.Shared/MoneyBee.Shared/MoneyBee.Shared.csproj", "src/MoneyBee.Shared/MoneyBee.Shared/"]
COPY ["src/MoneyBee.Auth/MoneyBee.Auth/MoneyBee.Auth.csproj", "src/MoneyBee.Auth/MoneyBee.Auth/"]
RUN dotnet restore "src/MoneyBee.Auth/MoneyBee.Auth/MoneyBee.Auth.csproj"
COPY . .
WORKDIR "/src/src/MoneyBee.Auth/MoneyBee.Auth"
RUN dotnet build "MoneyBee.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MoneyBee.Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoneyBee.Auth.dll"]
