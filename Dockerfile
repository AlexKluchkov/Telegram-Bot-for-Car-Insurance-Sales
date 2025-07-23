# Этап 1: сборка проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем только .csproj
COPY TelegramBot.csproj ./
RUN dotnet restore TelegramBot.csproj

# Копируем всё остальное
COPY . ./
RUN dotnet publish TelegramBot.csproj -c Release -o /out

# Этап 2: рантайм
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "TelegramBot.dll"]
