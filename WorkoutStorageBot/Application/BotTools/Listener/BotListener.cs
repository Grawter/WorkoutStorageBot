#region using

using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.BotTools.Logging;
using WorkoutStorageBot.Application.BotTools.Sender;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.Common;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Listener
{
    internal class BotListener
    {
        private readonly ITelegramBotClient botClient;

        private readonly IBotResponseSender botSender;

        private readonly CoreManager coreManager;

        private readonly ConfigurationData configurationData;

        private readonly ILogger<BotListener> logger;

        public BotListener(ITelegramBotClient botClient,
                           IBotResponseSender botSender,
                           CoreManager coreManager,
                           ConfigurationData configurationData,
                           ILoggerFactory loggerFactory)
        {
            this.botClient = CommonHelper.GetIfNotNull(botClient);
            this.botSender = CommonHelper.GetIfNotNull(botSender);

            this.coreManager = CommonHelper.GetIfNotNull(coreManager);

            this.configurationData = CommonHelper.GetIfNotNull(configurationData);
            ConfigurationManager.SetCensorToDBSettings(this.configurationData);

            this.logger = CommonHelper.GetIfNotNull(loggerFactory).CreateLogger<BotListener>();
        }

        internal async Task StartListen()
        {
            User bot = await botClient.GetMe();

            logger.LogInformation(EventIDHelper.GetNextEventId(CommonConsts.EventNames.StartingBot), $"Телеграм бот @{bot.Username} запущен");

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(), // receive all update types except ChatMember related updates
                DropPendingUpdates = true,
            };

            botClient.StartReceiving(updateHandler: HandleUpdateAsync,
                                     errorHandler: HandleErrorAsync,
                                     receiverOptions: receiverOptions,
                                     cancellationToken: coreManager.CancellationToken);
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            => await coreManager.ProcessUpdate(update);

        async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            EventId eventId = EventIDHelper.GetNextEventIdThreadSave(CommonConsts.EventNames.Critical);

            logger.Log(LogLevel.Critical,
                eventId,
                new Dictionary<string, object>(),
                exception,
                LogFormatter.CriticalExBotFormatter);

            if (configurationData.Notifications.NotifyOwnersAboutCriticalErrors)
                await botSender.SendSimpleMassiveResponse(configurationData.Bot.OwnersChatIDs, @$"{"Необработанная ошибка".AddBold()}:
{exception.ToString()}");
        }
    }
}