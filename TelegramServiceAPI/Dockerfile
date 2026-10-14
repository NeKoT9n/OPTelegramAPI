# Сборочный образ
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Сначала копируем файл решения (.sln), если он есть в корне, 
# или копируем все файлы проектов с сохранением структуры папок
COPY ["TelegramServiceAPI.slnx", "./"]
COPY ["TelegramServiceAPI/TelegramServiceAPI.csproj", "TelegramServiceAPI/"]
COPY ["TelegramServiceAPI.Core/TelegramServiceAPI.Core.csproj", "TelegramServiceAPI.Core/"]
COPY ["TelegramServiceAPI.Model/TelegramServiceAPI.Model.csproj", "TelegramServiceAPI.Model/"]
COPY ["TelegramServiceAPI.Infrastructure/TelegramServiceAPI.Infrastructure.csproj", "TelegramServiceAPI.Infrastructure/"]

# 2. Восстанавливаем зависимости для всего решения или главного проекта
RUN dotnet restore "TelegramServiceAPI/TelegramServiceAPI.csproj"

# 3. Копируем весь остальной исходный код
COPY . .

# 4. Собираем и публикуем проект
WORKDIR "/src/TelegramServiceAPI"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# Финальный рабочий образ
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TelegramServiceAPI.dll"]