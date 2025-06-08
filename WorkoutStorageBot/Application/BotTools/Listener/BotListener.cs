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
using WorkoutStorageBot.Helpers.Common;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Listener
{
    internal class BotListener
    {
        private readonly ITelegramBotClient _botClient;

        private readonly IBotResponseSender _botSender;

        private readonly CoreManager _coreManager;

        private readonly ConfigurationData _configurationData;

        private readonly ILogger<BotListener> _logger;

        public BotListener(ITelegramBotClient botClient,
                           IBotResponseSender botSender,
                           CoreManager coreManager,
                           ConfigurationData configurationData,
                           ILoggerFactory loggerFactory)
        {
            _botClient = CommonHelper.GetIfNotNull(botClient);
            _botSender = CommonHelper.GetIfNotNull(botSender);

            _coreManager = CommonHelper.GetIfNotNull(coreManager);

            _configurationData = CommonHelper.GetIfNotNull(configurationData);
            ConfigurationManager.SetCensorToDBSettings(_configurationData);

            _logger = CommonHelper.GetIfNotNull(loggerFactory).CreateLogger<BotListener>();
        }

        internal async Task StartListen()
        {
            User bot = await _botClient.GetMe();

            _logger.LogInformation(EventIDHelper.GetNextEventId(CommonConsts.EventNames.StartingBot), $"Телеграм бот @{bot.Username} запущен");

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(), // receive all update types except ChatMember related updates
                DropPendingUpdates = true,
            };

            _botClient.StartReceiving(updateHandler: HandleUpdateAsync,
                                     errorHandler: HandleErrorAsync,
                                     receiverOptions: receiverOptions,
                                     cancellationToken: _coreManager.CancellationToken);
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            => await _coreManager.ProcessUpdate(update);

        async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            EventId eventId = EventIDHelper.GetNextEventIdThreadSave(CommonConsts.EventNames.Critical);

            _logger.Log(LogLevel.Critical,
                eventId,
                new Dictionary<string, object>(),
                exception,
                LogFormatter.CriticalExBotFormatter);

            if (_configurationData.Notifications.NotifyOwnersAboutCriticalErrors)
                await _botSender.SendSimpleMassiveResponse(_configurationData.Bot.OwnersChatIDs, @$"<b>Необработанная ошибка</b>:
{exception.ToString()}");
        }
    }
}