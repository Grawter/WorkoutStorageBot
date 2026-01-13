using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Core.BotTools.Listener
{
    internal class BotListener
    {
        private readonly IServiceScopeFactory scopeFactory;

        private readonly IContextKeeper contextKeeper;

        private readonly ITelegramBotClient botClient;

        private readonly IBotResponseSender botSender;

        private readonly ConfigurationData configurationData;

        private CancellationTokenSource cancellationTokenSource;

        public BotListener(IServiceScopeFactory scopeFactory,
                           IContextKeeper contextKeeper,
                           ITelegramBotClient botClient,
                           IBotResponseSender botSender,
                           ConfigurationData configurationData)
        {
            this.scopeFactory = CommonHelper.GetIfNotNull(scopeFactory);
            this.contextKeeper = CommonHelper.GetIfNotNull(contextKeeper);
            this.botClient = CommonHelper.GetIfNotNull(botClient);
            this.botSender = CommonHelper.GetIfNotNull(botSender);

            this.configurationData = CommonHelper.GetIfNotNull(configurationData);
            ConfigurationManager.SetCensorToDBSettings(this.configurationData);
        }

        internal async Task StartListen(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = CommonHelper.GetIfNotNull(cancellationTokenSource);

            using IServiceScope scope = scopeFactory.CreateScope();

            ILogger logger = GetLoggerOnScope(scope);

            User bot = await botClient.GetMe();

            if (configurationData.DB.EnsureCreated)
                EnsureCreatedDB(scope);

            logger.LogInformation(EventIDHelper.GetNextEventId(CommonConsts.EventNames.StartingBot), $"Телеграм бот @{bot.Username} запущен");

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(), // receive all update types except ChatMember related updates
                DropPendingUpdates = true,
            };

            botClient.StartReceiving(updateHandler: HandleUpdateAsync,
                                     errorHandler: HandleErrorAsync,
                                     receiverOptions: receiverOptions,
                                     cancellationToken: this.cancellationTokenSource.Token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using IServiceScope scope = scopeFactory.CreateScope();

            EntityContext db = scope.ServiceProvider.GetRequiredService<EntityContext>();
            ICustomLoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ICustomLoggerFactory>();

            CoreManager coreManager = new CoreManager(configurationData, db, contextKeeper, botSender, loggerFactory, cancellationTokenSource);

            await coreManager.ProcessUpdate(update);
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();

                ILogger logger = GetLoggerOnScope(scope);

                EventId eventId = EventIDHelper.GetNextEventIdThreadSave(CommonConsts.EventNames.Critical);

                logger.Log(LogLevel.Critical,
                           eventId,
                           new Dictionary<string, object>(),
                           exception,
                           LogFormatter.CriticalExBotFormatter);

                logger.LogWarning("Отключение бота после необработанной ошибки");

                if (configurationData.Notifications.NotifyOwnersAboutCriticalErrors)
                    await botSender.SendSimpleMassiveResponse(configurationData.Bot.OwnersChatIDs, @$"!!!Необработанная ошибка!!!:
{exception.ToString()}");
            }
            finally
            {
                cancellationTokenSource.Cancel();
            }
        }

        private ILogger GetLoggerOnScope(IServiceScope scope)
        {
            ICustomLoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ICustomLoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger<BotListener>();

            return logger;
        }

        private void EnsureCreatedDB(IServiceScope scope)
        {
            EntityContext db = scope.ServiceProvider.GetRequiredService<EntityContext>();
            db.Database.EnsureCreated();
        }
    }
}