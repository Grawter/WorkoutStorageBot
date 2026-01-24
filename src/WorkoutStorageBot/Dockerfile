# Сборка
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Необязательно, кэш сборки
COPY WorkoutStorageBot/WorkoutStorageBot.csproj WorkoutStorageBot/
COPY WorkoutStorageModels/WorkoutStorageModels.csproj WorkoutStorageModels/

RUN dotnet restore WorkoutStorageBot/WorkoutStorageBot.csproj

COPY . .

WORKDIR /src/WorkoutStorageBot

RUN dotnet publish -c Release -o /app/publish --no-restore

# Запуск
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY --from=build /app/publish .

# Создание папки data (на случай, если volume пустой)
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "WorkoutStorageBot.dll"]