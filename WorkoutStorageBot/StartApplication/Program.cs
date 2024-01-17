#region using
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Helpers.Logger;
using WorkoutStorageBot.BusinessLogic.Handlers;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.StartApplication
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls13;

            ILogger logger = new ConsoleLogger();

            string pathFileConfig = "appsettings.json";
            if (!System.IO.File.Exists(pathFileConfig))
            {
                logger.WriteLog($"Указанный config-файл {pathFileConfig} не найден", LogType.StartError);
                return;
            }

            var configurationBuilder = new ConfigurationBuilder()
                                                                .AddJsonFile(pathFileConfig, optional: false, reloadOnChange: true)
                                                                .AddUserSecrets<Program>();
            var configuration = configurationBuilder.Build();

            var token = configuration["Telegram:Token"];
            if (string.IsNullOrEmpty(token))
            {
                logger.WriteLog("Получен пустой токен", LogType.StartError);
                return;
            }

            var ownerChatId = configuration["Telegram:OwnerChatId"];
            if (string.IsNullOrEmpty(token))
            {
                logger.WriteLog("Получена пустая строка идентификатора чата владельца", LogType.StartError);
                return;
            }

            var connectionString = configuration["DB:Database"];
            if (string.IsNullOrEmpty(token))
            {
                logger.WriteLog("Получена пустая строка подключения к БД", LogType.StartError);
                return;
            }

            var optionsBuilder = new DbContextOptionsBuilder<EntityContext>();
            var options = optionsBuilder.UseSqlite("Data Source=" + connectionString).Options;

            var botClient = new TelegramBotClient(token);

            var db = new EntityContext(options);

            var adminHandler = new AdminHandler(db);

            var telegramBotHandler = new TelegramBotHandler(botClient, db, logger, adminHandler);

            using CancellationTokenSource cts = new();

            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(), // receive all update types except ChatMember related updates
                ThrowPendingUpdates = true
            };

            botClient.StartReceiving(updateHandler: HandleUpdateAsync,
                                    pollingErrorHandler: HandlePollingErrorAsync,
                                    receiverOptions: receiverOptions,
                                    cancellationToken: cts.Token
                                    );

            var me = await botClient.GetMeAsync();

            logger.WriteLog($"Телеграм бот @{me.Username} запущен", LogType.Information);

            string[] command;
            UserInformation? user;

            while (true)
            {
                command = Console.ReadLine().ToLower().Trim().Split(' ');

                switch (command[0])
                {
                    case "switchmode":
                        switch (command[1])
                        {
                            case "wlpr":
                                if (telegramBotHandler.WhiteList)
                                {
                                    telegramBotHandler.WhiteList = false;
                                    logger.WriteLog("Режим белого списка выключён", LogType.Admin);
                                }
                                else
                                {
                                    telegramBotHandler.WhiteList = true;
                                    logger.WriteLog("Режим белого списка включён", LogType.Admin);
                                }
                                break;
                            default:
                                logger.WriteLog($"Неизвестная команда после switchmode: {command[1]}", LogType.Admin);
                                break;
                        }
                        break;

                    case "changestate":
                        user = adminHandler.GetUserInformation(long.Parse(command[2]));
                        if (user == null)
                        {
                            logger.WriteLog($"Пользователь с userId {user.UserId} не найден", LogType.Admin);
                            break;
                        }

                        switch (command[1])
                        {
                            case "wl":
                                adminHandler.ChangeWhiteList(user);

                                logger.WriteLog($"WhiteList для {user.Username} {user.UserId} установлен на {user.WhiteList}", LogType.Admin);
                                break;
                            case "bl":
                                adminHandler.ChangeBlackList(user);

                                logger.WriteLog($"BlackList для {user.Username} {user.UserId} установлен на {user.BlackList}", LogType.Admin);
                                break;
                            default:
                                logger.WriteLog($"Неизвестная команда после changestate: {command[1]}", LogType.Admin);
                                break;
                        }
                        break;

                    case "rm":
                        user = adminHandler.GetUserInformation(long.Parse(command[2]));
                        if (user == null)
                        {
                            logger.WriteLog($"Пользвоатель с userId {user.UserId} не найден", LogType.Admin);
                            break;
                        }

                        switch (command[1])
                        {
                            case "user":
                                adminHandler.DeleteAccount(user);
                                logger.WriteLog($"Пользователь {user.Username} {user.UserId} удалён", LogType.Admin);
                                break;
                            default:
                                logger.WriteLog($"Неизвестная команда после rm: {command[1]}", LogType.Admin);
                                break;
                        }
                        break;

                    case "stopbot":
                        cts.Cancel(); // Send cancellation request to stop bot
                        logger.WriteLog($"Телеграм бот @{me.Username} остановлен из консоли", LogType.Admin);
                        return;

                    case "commands":
                        logger.WriteLog("\nswitchmode wlpr - переключить режим белого листа\n" +
                                        "changestate [wl/bl] userId - установить состояние\n" +
                                        "rm user userId - удалить пользователя\n", 
                                        LogType.Admin);
                        break;
                    default:
                        logger.WriteLog("Неизвестная команда. Введите commands для просмотра доступных команд", LogType.Admin);
                        break;

                }
            }

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                try
                {
                    await telegramBotHandler.ProcessUpdate(update);
                }
                catch (Exception ex)
                {
                    logger.WriteLog(ex.ToString(), LogType.RunTimeError);
                }
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var errorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                logger.WriteLog(errorMessage, LogType.CriticalError);

                botClient.SendTextMessageAsync(ownerChatId, $"Аварийное завершение приложения.\n {errorMessage}");

                return Task.CompletedTask;
            }
        }
    }
}