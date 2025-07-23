# Этап 1: сборка проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем только .csproj
COPY BotForCarInsuranceSales.csproj ./
RUN dotnet restore BotForCarInsuranceSales.csproj

# Копируем всё остальное
COPY . ./
RUN dotnet publish BotForCarInsuranceSales.csproj -c Release -o /out

# Этап 2: рантайм
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "TelegramBot.dll"]
