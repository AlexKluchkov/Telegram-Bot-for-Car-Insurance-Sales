
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build


COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

COPY --from=build /out ./

ENTRYPOINT ["dotnet", "BotForCarInsuranceSales.dll"]
