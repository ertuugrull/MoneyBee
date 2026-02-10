FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MoneyBee.Shared/MoneyBee.Shared/MoneyBee.Shared.csproj", "src/MoneyBee.Shared/MoneyBee.Shared/"]
COPY ["src/MoneyBee.Customer/MoneyBee.Customer/MoneyBee.Customer.csproj", "src/MoneyBee.Customer/MoneyBee.Customer/"]
RUN dotnet restore "src/MoneyBee.Customer/MoneyBee.Customer/MoneyBee.Customer.csproj"
COPY . .
WORKDIR "/src/src/MoneyBee.Customer/MoneyBee.Customer"
RUN dotnet build "MoneyBee.Customer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MoneyBee.Customer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoneyBee.Customer.dll"]
