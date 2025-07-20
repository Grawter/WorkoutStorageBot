# :robot: WorkoutStorageBot

������� Telegram-��� ��� �������, �������� � ��������� ����������� ����������

## :pushpin: ��������

**WorkoutStorageBot** � �����:

- ������� ���������� ����������, �������� �� �� ����� � ���
- ���������� ��������� ������ ����������, � ��� �� ����������� ���������� �� ����
- �������������� ������ ���������� � .xlsx � .json �����
- ����������� ����������� �����, ���, ����������

---

## :rocket: Quick Start

����� ������ ��������� ����, �������� ���� �����:

### 1. ���������� �����������

```
git clone https://github.com/Grawter/WorkoutStorageBot.git
```

### 2. ��������� ��������� ���������

```
cd WorkoutStorageBot/WorkoutStorageBot/Application/StartConfiguration
nano appsettings.json
```

������� ����� ������ �������� ���� � Bot->Token

```
{
  "DB": {
    "Server": "",
    "Database": "test.db",
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

### 3. �������� � ��������� ����

```
cd ../..
dotnet build
dotnet run
```

### 4. �������� ���� ������ ������������� ����
![til](./DescriptionResources/StartBot.gif)


## :world_map: ����� ������
![�������� ��������� �����](./DescriptionResources/ButtonsMap.jpg)

## :jigsaw: ������ �������� ��������� appsettings.json

```
{
  // ��������� ���������� �������� ��� ���� ����� ����� � ConfigurationData -> GetConfiguration

  /*
  ��������� ����������� � ��.
  ��-���������, ������������ SQLite, � ������ ������������� �������� DBProvider: Program -> GetEntityContext.
  � ������ ������������� ����� �������� ���� ���������� ������ �����������: ConfigurationData -> DbSettings -> ConnectionString.

  Server - ������ DB [�������������]
  Database - �������� DB [�����������]
  UserName - �� ��� ����������� � DB [�������������]
  Password - ��� �� �� ��� ����������� � DB [�������������]
  
  ����� ������������� ���� ������������ UserName � Password ����� ����� �� ����������. ���������: ConfigurationManager -> SetCensorToDBSettings.
  */
  "DB": {
    "Server": "",
    "Database": "test.db",
    "UserName": "",
    "Password": ""
  },

  /*
  ��������� ���������������� ����.

  Token - ����� ��� ���������� ����� [�����������]
  WhiteListIsEnable - ����� �� �������� ����� ������ ������ [�����������]
  OwnersChatIDs - �������� ID ������� ���� (��� �������� � �������) [�����������]
  IsNeedCacheContext - ����� �� �������� ��� ��� ����������� ��������� [�����������]
  ��� ���������� ������ ��-���������, �� �������� 6 �����, ���� � ������������ ��������� �� ���� ��������.
  IsNeedLimits - ����� �� �������� ����� �������. [�������������] [��-��������� - False]
  */
  "Bot": {
    "Token": "<Your telegram bot token>",
    "WhiteListIsEnable": "False",
    "OwnersChatIDs": [ "000000000" ],
    "IsNeedCacheContext": "True",
    "IsNeedLimits": "True"
  },

  /*
  ��������� ����������� �� ���������.

  NotifyOwnersAboutCriticalErrors - �������� ����������� �� ����������� ������ � ���� ������� [�����������]
  NotifyOwnersAboutCriticalErrors - �������� ����������� �� ����������� ������ � ���� ������� [�����������]
  */
  "Notifications": {
    "NotifyOwnersAboutCriticalErrors": "True",
    "NotifyOwnersAboutRuntimeErrors": "True"
  },

  /*
  ��������� ������������ ����������.

  ��������� ������ ������������: Trace, Debug, Information, Warning, Error, Critical, None, All.

  MainRuleLog - �������� ������� ������������. ��-��������� ��� ������������ �� ���� �������.
  DBLogLevels - ���� � ���� ������� ����� �������� � �� [�������������]
  ConsoleLogLevels - ���� � ���� ������� ����� �������� � ������� [�������������]
  
  CustomRulesLog - ��������� ������� ������������ ������ ��� ������-�� ������ [�������������]
  FullClassName - ������ �������� ������ [�����������, ���� ����������� ��������� �������]
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
  AboutBot - ��������, ��������� �� ���� ��������� �� ������ "� ����" [�������������]
  */
  "AboutBot": "Some information about bot"
}
```

## :toolbox: �������������� ����������

- �������� ������: ������������ SQLite + EF
- ���� ��������� ������� "Logs"
- ������ ���������� ����� ������������� ����� ���������� ���������� **WorkoutStorageImport**. 
������ �� ��������������� ������ �������� � ������� "ImportInfo"