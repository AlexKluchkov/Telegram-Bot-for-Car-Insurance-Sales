
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY BotForCarInsuranceSales.csproj ./
RUN dotnet restore BotForCarInsuranceSales.csproj

COPY . ./
RUN dotnet publish BotForCarInsuranceSales.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "TelegramBot.dll"]
