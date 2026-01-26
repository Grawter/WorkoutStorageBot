# :robot: WorkoutStorageBot

Простой Telegram-бот для записей, хранения и просмотра результатов тренировок

## :pushpin: Описание

**WorkoutStorageBot** — умеет:

- Хранить результаты тренировок, разбивая их на циклы и дни
- Показывать последние данные тренировок, а так же фильтровать результаты по дням
- Экспортировать данные тренировок в .xlsx и .json файлы
- Динамически настраивать циклы, дни, упражнения

---

## :rocket: Quick Start

Чтобы быстро запустить бота, следуйте этим шагам:

### 1. Клонируйте репозиторий

```
git clone https://github.com/Grawter/WorkoutStorageBot.git
```

### 2. Заполните стартовую настройку

```
cd src/WorkoutStorageBot/WorkoutStorageBot/Application/StartConfiguration
nano appsettings.json
```

Укажите токен своего телеграм бота в Bot->Token

```
{
  "DB": {
    "EnsureCreated": "True",
    "Server": "",
    "Database": "data/test.db",
    "UserName": "",
    "Password": ""
  },

  "Bot": {
    "Token": "<Your telegram bot token>",
    "WhiteListIsEnable": "False",
    "OwnersChatIDs": [ "0" ],
    "IsNeedCacheContext": "False",
    "IsNeedLimits": "False"
  },

  "Notifications": {
    "NotifyOwnersAboutCriticalErrors": "False",
    "NotifyOwnersAboutRuntimeErrors": "False"
  },

  "LogInfo": {
    "MainRuleLog": {
      "DBLogLevels": [ "Error", "Critical" ],
      "ConsoleLogLevels": [ "All" ]
    }
  }
}
```

### 3. Соберите и запустите бота

```
cd ../../..
docker-compose up -d
```

### 4. Создайте свой первый тренировочный цикл
![til](./CommonResources/DescriptionResources/StartBot.gif)


## :world_map: Карта кнопок
![Описание элементов схемы](./CommonResources/DescriptionResources/ButtonsMap.jpg)

## :jigsaw: Полное описание настройки appsettings.json

```
{
  // Настроить считывание настроек под свои нужды можно в ConfigurationData -> GetConfiguration

  /*
  Настройки подключения к БД.
  По-умолчанию, используется SQLite, в случае необходимости можно поменять DBProvider: Program -> GetEntityContext.
  В случае необходимости можно написать свою реализацию строки подключения: ConfigurationData -> DbSettings -> ConnectionString.

  EnsureCreated - Требуется ли первичная инициализация БД при старте приложения (Database.EnsureCreated()) [Необязательно] [По-умолчанию - False]
  Server - Сервер DB [Необязательно]
  Database - Название DB. data обязательно, т.к. в этой папке будет volume [Обязательно]
  UserName - УЗ для подключения к DB [Необязательно]
  Password - для от УЗ для подключения к DB [Необязательно]
  
  После инициализации всех зависимостей значения UserName и Password будут затёрты из переменных. Подробнее: ConfigurationManager -> SetCensorToDBSettings.
  */
  "DB": {
    "EnsureCreated": "False",
    "Server": "",
    "Database": "data/<someName>.db",
    "UserName": "",
    "Password": ""
  },

  /*
  Настройки функциональности бота.

  Token - Токен для управления ботом [Обязательно]
  WhiteListIsEnable - Нужно ли включать режим белого списка [Обязательно]
  OwnersChatIDs - Телеграм ID админов бота (для доступка к админке) [Обязательно]
  IsNeedCacheContext - Нужно ли включать кэш для глобального контекста [Обязательно]
  При включённом режиме, по-умолчанию, он держится 6 часов, если к определённому контексту не было запросов.
  IsNeedLimits - Нужно ли включать режим лимитов. [Необязательно] [По-умолчанию - False]
  IsSupportOnlyKnownTypes - Поддерживать только известные типы обновлений (UpdateType.Message, UpdateType.CallbackQuery). [Необязательно] [По-умолчанию - True]
  */
  "Bot": {
    "Token": "<Your telegram bot token>",
    "WhiteListIsEnable": "False",
    "OwnersChatIDs": [ "000000000" ],
    "IsNeedCacheContext": "True",
    "IsNeedLimits": "True",
    "IsSupportOnlyKnownTypes": "True"
  },

  /*
  Настройки уведомлений об операциях.

  NotifyOwnersAboutCriticalErrors - Отсылать уведомления админам об критических ошибках в боте [Обязательно]
  NotifyOwnersAboutRuntimeErrors - Отсылать уведомления админам об некритических ошибках в боте [Обязательно]
  */
  "Notifications": {
    "NotifyOwnersAboutCriticalErrors": "True",
    "NotifyOwnersAboutRuntimeErrors": "True"
  },

  /*
  Настройки логгирования приложения.

  Доступные уровни логгирования: Trace, Debug, Information, Warning, Error, Critical, None, All.

  MainRuleLog - Основное правило логгирование. По-умолчанию, для логгирования из всех классов.
  DBLogLevels - Логи с этим уровнем будут записаны в БД [Необязательно]
  ConsoleLogLevels - Логи с этим уровнем будут выведены в консоли [Необязательно]
  
  CustomRulesLog - Кастомное правило логгирования, строго для какого-то класса [Необязательно]
  FullClassName - Полное название класса [Обязательно, если указывается кастомное правило]
  */
  "LogInfo": {
    "MainRuleLog": {
      "DBLogLevels": [ "Error", "Critical" ],
      "ConsoleLogLevels": [ "All" ]
    },

    "CustomRulesLog": [
      {
        "FullClassName": "",
        "DBLogLevels": [ "" ],
        "ConsoleLogLevels": [ "" ]
      }
    ]
  },

  /*
  AboutBot - Описание, выводимое из меню настройки по кнопке "О боте" [Необязательно]
  */
  "AboutBot": "Some information about bot"
}
```

## :briefcase: Database Schema

```
Domains:
+---------------------+      +-------------------------------+      +---------------------+      +-----------------------+      +------------------------+
|   UserInformation   | 1  * |             Cycle             | 1  * |         Day         | 1  * |       Exercise        | 1  * |     ResultExercise     |
+---------------------+------>-------------------------------+------>---------------------+------>-----------------------+------>------------------------+
| Id (PK) : int       |      | Id (PK) : int                 |      | Id (PK) : int       |      | Id (PK) : int         |      | Id (PK) : int          |
| UserId : long       |      | Name : string                 |      | Name : string       |      | Name : string         |      | Count : int?           |
| FirstName : string  |      | UserInformationId (FK) : int  |      | CycleId (FK) : int  |      | Mode : ExercisesMods  |      | Weight : float?        |
| Username : string   |      | IsActive : bool               |      | IsArchive : bool    |      | DayId (FK) : int      |      | FreeResult : string?   |
| WhiteList : bool    |      | IsArchive : bool              |      |                     |      | IsArchive : bool      |      | DateTime : DateTime    |
| BlackList : bool    |      |                               |      |                     |      |                       |      | ExerciseId (FK) : int  |
+---------------------+      +-------------------------------+      +---------------------+      +-----------------------+      +------------------------+

==========================================================================================================================================================

Infrastructure Tables:
+-------------------------+      +-------------------------------+
|           Log           |      |          ImportInfo           |
+-------------------------+      +-------------------------------+
| Id (PK) : int           |      | Id (PK) : int                 |
| LogLevel : string       |      | DomainType : string           |
| EventID : int?          |      | DomainId : int                |
| EventName : string?     |      | UserInformationId (FK) : int  |
| DateTime : DateTime     |      | DateTime : DateTime           |
| Message : string        |      |                               |
| SourceContext : string  |      |                               |
| TelegramUserId : long?  |      |                               |
+-------------------------+      +-------------------------------+

==================================================================

Enums in Tables:
+------------------+
|  ExercisesMods   |
+------------------+
| Count = 0        |
| WeightCount = 1  |
| Timer = 2        |
| FreeResult = 3   |
+-------------------+
```
### :clipboard: Описание таблиц
**Domains**

- UserInformation — Профили пользователей

- Cycle — Тренировочные циклы пользователей

- Day — Тренировочные дни внутри цикла

- Exercise — Упражнения внутри тренировочного дня

- ResultExercise — Результаты выполнения упражнений

**Infrastructure Tables**

- ImportInfo - Хранит историю об импорте доменных сущностей

- Log - Таблица логов (инфраструктурная, без FK)

## :toolbox: Дополнительная информация

- Хранение данных: Используется SQLite + EF
- **WorkoutStorageBot** - Cам бот и его бизнес-логика
- **WorkoutStorageImport** - Утилита для экспорта тренировок из json
- **WorkoutStorageModels** - Модели EF
- **WorkoutStorageBot.UnitTests** - Unit тесты бота