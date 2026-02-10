FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MoneyBee.Shared/MoneyBee.Shared/MoneyBee.Shared.csproj", "src/MoneyBee.Shared/MoneyBee.Shared/"]
COPY ["src/MoneyBee.Transfer/MoneyBee.Transfer/MoneyBee.Transfer.csproj", "src/MoneyBee.Transfer/MoneyBee.Transfer/"]
RUN dotnet restore "src/MoneyBee.Transfer/MoneyBee.Transfer/MoneyBee.Transfer.csproj"
COPY . .
WORKDIR "/src/src/MoneyBee.Transfer/MoneyBee.Transfer"
RUN dotnet build "MoneyBee.Transfer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MoneyBee.Transfer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoneyBee.Transfer.dll"]
