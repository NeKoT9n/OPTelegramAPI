FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TelegramServiceAPI/TelegramServiceAPI.csproj", "TelegramServiceAPI/"]
COPY ["TelegramServiceAPI.Core/TelegramServiceAPI.Core.csproj", "TelegramServiceAPI.Core/"]
COPY ["TelegramServiceAPI.Model/TelegramServiceAPI.Model.csproj", "TelegramServiceAPI.Model/"]

RUN dotnet restore "TelegramServiceAPI/TelegramServiceAPI.csproj"

COPY . .
WORKDIR "/src/TelegramServiceAPI"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TelegramServiceAPI.dll"]