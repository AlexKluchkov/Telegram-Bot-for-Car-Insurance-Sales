# Use the official .NET SDK for assembly
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR ./

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all the rest of the code and compile
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

WORKDIR ./
COPY --from=build /out ./

# Launch the application
ENTRYPOINT ["dotnet", "TelegramBot.dll"]
