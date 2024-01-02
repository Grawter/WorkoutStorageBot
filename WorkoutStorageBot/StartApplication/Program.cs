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

            var builder = new ConfigurationBuilder()
                                                .AddJsonFile(pathFileConfig, optional: false, reloadOnChange: true)
                                                .AddUserSecrets<BackingFieldAttribute>();
            var configuration = builder.Build();

            var token = configuration["Telegram:Token"];
            if (string.IsNullOrEmpty(token))
            {
                logger.WriteLog("Получен пустой токен", LogType.StartError);
                return;
            }

            var OwnerChatId = configuration["Telegram:OwnerChatId"];
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

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            var options = optionsBuilder.UseSqlite("Data Source=" + connectionString).Options;

            var botClient = new TelegramBotClient(token);

            var telegramBotHandler = new TelegramBotHandler(botClient, options, logger);

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

            while (true)
            {
                switch (Console.ReadLine().ToLower().Trim())
                {
                    case "/add wl":

                        break;
                    case "/rm wl":

                        break;

                    case "/add bl":

                        break;
                    case "/rm bl":

                        break;

                    case "/rm user":

                        break;

                    case "/switch wl":
                        if (telegramBotHandler.WhiteList)
                        {
                            telegramBotHandler.WhiteList = false;
                            logger.WriteLog($"Режим белого списка выключён", LogType.Admin);
                        }
                        else
                        {
                            telegramBotHandler.WhiteList = true;
                            logger.WriteLog($"Режим белого списка включён", LogType.Admin);
                        }

                        break;
                    case "/stopbot":
                        cts.Cancel(); // Send cancellation request to stop bot
                        logger.WriteLog($"Телеграм бот @{me.Username} остановлен из консоли", LogType.Admin);
                        return;
                    case "/commands":

                        break; ;
                    default:
                        logger.WriteLog("Неизвестная команда. Введите /commands для просмотра доступных команд");
                        break;

                }
            }

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                try
                {
                    await Task.Run(() => telegramBotHandler.ProcessUpdate(update));
                }
                catch (Exception ex)
                {
                    logger.WriteLog(ex.ToString(), LogType.RunTimeError);
                }
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                logger.WriteLog(ErrorMessage, LogType.CriticalError);

                botClient.SendTextMessageAsync(OwnerChatId, $"Аварийное завершение приложения.\n {ErrorMessage}");

                return Task.CompletedTask;
            }
        }
    }

}