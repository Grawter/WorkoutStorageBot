using Microsoft.EntityFrameworkCore;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageImport
{
    internal class Program
    {
        private static ConsoleColor originalConsoleColor = ConsoleColor.Gray;

        static void Main(string[] args)
        {
            EntityContext entityContext = GetValueFromInput("Введите connectionString для БД", GetEntityContext); // GetEntityContext("Data Source= Workout Storage.db");

            string workoutStr = GetValueFromInput("Введите расположение .json файла с тренироваками для миграции в ДБ", GetWorkoutStr); // "./Workout.json";

            int typeID = GetValueFromInput(@"Введите тип условия, по которому искать пользователя для импорта тренировок. Введите:
1, если UsersInformationID
2, если telegramUserID
3, если telegramUserName", GetTypeID);

            UserInformation userInformation = GetValueFromInput($"Введите условия для поиска по типу '{typeID}'", GetUserInformation, typeID, entityContext);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Все данные получены. Для начала импортна нажмите любую кнопку");
            Console.ReadKey();

            ExecuteImport(entityContext, workoutStr, userInformation);

            Console.WriteLine("Импорт тренировок завершён");
            Console.ForegroundColor = originalConsoleColor;
        }

        private static T GetValueFromInput<T>(string startMessage, Func<string, object[], T> func, params object[] additionParam)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(startMessage);
            ArgumentNullException.ThrowIfNull(func);

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine(startMessage);

                Console.ForegroundColor = ConsoleColor.Yellow;

                string? inputStr = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(inputStr))
                {
                    try
                    {
                        T value = func(inputStr, additionParam);

                        Console.ForegroundColor = originalConsoleColor;

                        return value;
                    }
                    catch (ExpectedException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine(ex.Message);
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;

                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;

                    Console.WriteLine("Получена пустая строка");
                }
                    
            }
        }

        private static EntityContext GetEntityContext(string connectionString, params object[] additionalParams)
        {
            DbContextOptionsBuilder<EntityContext> optionsBuilder = new DbContextOptionsBuilder<EntityContext>();
            DbContextOptions<EntityContext> options = optionsBuilder.UseSqlite(connectionString).Options;
            EntityContext db = new EntityContext(options);

            return db;
        }

        private static string GetWorkoutStr(string pathToFileWithWorkout, params object[] additionalParams)
        {
            if (!Path.Exists(pathToFileWithWorkout))
                throw new ExpectedException($"Не удалось найти файл по пути '{pathToFileWithWorkout}'");

            string fileExtension = Path.GetExtension(pathToFileWithWorkout);

            string expectedFileExtension = ".json";

            if (fileExtension != expectedFileExtension)
                throw new ExpectedException($"Расширение файла, указанного по пути '{pathToFileWithWorkout}' не соответствует '{expectedFileExtension}'");

            string workoutStr = File.ReadAllText(pathToFileWithWorkout);

            return workoutStr;
        }

        private static int GetTypeID(string typeIDStr, params object[] additionalParams)
        {
            if (!int.TryParse(typeIDStr, out int typeID))
                throw new ExpectedException($"Не удалось распарсить в int значение '{typeIDStr}'");

            if (typeID < 1 || typeID > 3)
                throw new ExpectedException($"Некорректное значение '{typeIDStr}'");

            return typeID;
        }

        private static UserInformation GetUserInformation(string userCondition, params object[] additionalParams)
        {
            int typeCondition = Convert.ToInt32(additionalParams[0]);

            EntityContext entityContext = (EntityContext)additionalParams[1];

            UserInformation? userInformation = null;

            switch (typeCondition)
            {
                case 1:
                    int ID = Convert.ToInt32(userCondition);

                    userInformation = entityContext.UsersInformation.FirstOrDefault(x => x.Id == ID);
                    break;
                case 2:
                    long telegramID = Convert.ToInt64(userCondition);

                    userInformation = entityContext.UsersInformation.FirstOrDefault(x => x.UserId == telegramID);
                    break;
                case 3:
                    string userName = Convert.ToString(userCondition);

                    userInformation = entityContext.UsersInformation.FirstOrDefault(x => string.Equals(x.Username, userName, StringComparison.Ordinal));
                    break;
                default:
                    throw new ExpectedException($"Неизвестный тип условия для поиска UserInformatin: '{typeCondition}'");
            }

            if (userInformation == null)
                throw new ExpectedException($"Не удалось найти UserInformation по '{userCondition}' c типом '{typeCondition}'");

            return userInformation;
        }

        private static void ExecuteImport(EntityContext entityContext, string inputStr, UserInformation userInformation)
        {
            ImportManager migrationManager = new ImportManager(entityContext);
            migrationManager.ExecuteImport(userInformation, inputStr);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(migrationManager.ImportResult);
        }
    }
}