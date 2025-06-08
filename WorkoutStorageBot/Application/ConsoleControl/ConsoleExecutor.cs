using WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.Domain;

namespace WorkoutStorageBot.Application.ConsoleControl
{
    internal class ConsoleExecutor
    {
        public ConsoleExecutor(CoreManager coreManager)
        {
            this.CoreManager = CommonHelper.GetIfNotNull(coreManager);
            this.AdminRepository = CoreManager.GetRepository<AdminRepository>()!;
        }

        private CoreManager CoreManager { get; }
        private AdminRepository AdminRepository { get; }

        internal void StartListen()
        {
            //await Task.Delay(TimeSpan.FromSeconds(3));

            while (true)
            {
                if (CoreManager.CancellationToken.IsCancellationRequested)
                    break;

                string? command = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(command))
                    TryExecuteCommand(command);

                if (CoreManager.CancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private async Task TryExecuteCommand(string command)
        {
            ConsoleCommandParser consoleCommandParser = new ConsoleCommandParser(command);

            switch (consoleCommandParser.Act)
            {
                case "change":
                    switch (consoleCommandParser.Object)
                    {
                        case "access":
                            AdminRepository.ChangeWhiteListMode();

                            break;

                        case "state":

                            string userData = consoleCommandParser.Args[2];

                            UserInformation? user = GetUserInformationWithLog(userData);

                            if (user != null)
                            {
                                string newState = consoleCommandParser.Args[3];

                                switch (newState)
                                {
                                    case "wl":
                                        AdminRepository.ChangeWhiteListByUser(user);
                                        break;
                                    case "bl":
                                        AdminRepository.ChangeBlackListByUser(user);
                                        break;
                                    default:
                                        Console.WriteLine($"Неизвестная команда после change state [userId/@Username]: '{newState}'. Укажите [wl/bl]");
                                        break;
                                }
                            }

                            break;

                        default:
                            Console.WriteLine($"Неизвестная команда после change: '{consoleCommandParser.Act}'. Введите 'commands' для просмотра всех доступных команд");
                            break;
                    }
                    break;

                case "rm":
                    switch (consoleCommandParser.Object)
                    {
                        case "user":

                            string userData = consoleCommandParser.Args[2];

                            UserInformation? user = GetUserInformationWithLog(userData);

                            if (user != null)
                                AdminRepository.DeleteAccount(user);

                            break;

                        default:
                            Console.WriteLine($"Неизвестная команда после rm: '{consoleCommandParser.Object}'. Введите 'commands' для просмотра всех доступных команд");
                            break;
                    }
                    break;

                case "stopbot":
                    await CoreManager.StopManaging(TimeSpan.FromSeconds(2));
                    return;

                case "commands":
                    Console.WriteLine(@"
change access - переключить режим белого списка
change state [userId/@Username] [wl/bl] - установить состояние для пользователя
rm user [userId/@Username] - удалить пользователя
stopbot - остановить бота");
                    break;

                default:
                    Console.WriteLine($"Неизвестная команда '{consoleCommandParser.Act}'. Введите 'commands' для просмотра всех доступных команд");
                    break;
            }
        }

        private UserInformation? GetUserInformationWithLog(string user)
        {
            UserInformation? userInformation = GetUserInformation(user);

            if (userInformation == null)
                Console.WriteLine($"Пользователь '{user}' не найден");

            return userInformation;
        }

        private UserInformation? GetUserInformation(string user)
        {
            UserInformation? userInformation = default;

            if (string.IsNullOrWhiteSpace(user))
            {
                if (user.StartsWith('@'))
                    userInformation = AdminRepository.GetUserInformation(user);
                else
                    userInformation = AdminRepository.GetUserInformation(long.Parse(user));
            }

            return userInformation;
        }
    }
}