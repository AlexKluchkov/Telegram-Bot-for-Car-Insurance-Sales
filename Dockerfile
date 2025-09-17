FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BotForCarInsuranceSales.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish BotForCarInsuranceSales.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /out ./
COPY Auto_Insurance_Policy_Template.txt ./

ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "BotForCarInsuranceSales.dll"]
