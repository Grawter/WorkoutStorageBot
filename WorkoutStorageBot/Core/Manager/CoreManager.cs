#region using

using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WorkoutStorageBot.Application.BotTools.Logging;
using WorkoutStorageBot.Application.BotTools.Sender;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.BusinessLogic.GlobalContext;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.EventIDHelper;
using WorkoutStorageBot.Helpers.Updates;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.Entities.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

#endregion using

namespace WorkoutStorageBot.Core.Manager
{
    internal class CoreManager
    {
        internal CoreManager(ConfigurationData configurationData,
                             EntityContext db,
                             IContextKeeper contextKeeper,
                             IBotResponseSender botResponseSender,
                             ICustomLoggerFactory loggerFactory,
                             CancellationTokenSource cancellationTokenSource)
        {
            this.ConfigurationData = CommonHelper.GetIfNotNull(configurationData);

            this.loggerFactory = CommonHelper.GetIfNotNull(loggerFactory);

            this.logger = this.loggerFactory.CreateLogger<CoreManager>();

            this.BotResponseSender = CommonHelper.GetIfNotNull(botResponseSender);

            this.cancellationTokenSource = CommonHelper.GetIfNotNull(cancellationTokenSource);

            CoreTools coreTools = new CoreTools()
            {
                Db = db,
                LoggerFactory = loggerFactory,
                ConfigurationData = configurationData
            };

            this.Repositories = new List<CoreRepository>()
            {
                new AdminRepository(coreTools, this),
                new LogsRepository(coreTools, this),
            };

            this.Handlers = new List<CoreHandler>()
            {
                new PrimaryUpdateHandler(coreTools, this, contextKeeper),
                new UpdateHandler(coreTools, this),
            };
        }

        internal List<CoreHandler> Handlers { get; }
        internal List<CoreRepository> Repositories { get; }

        private ConfigurationData ConfigurationData { get; }

        private IBotResponseSender BotResponseSender { get; }

        private readonly ICustomLoggerFactory loggerFactory;

        private readonly ILogger logger;

        private readonly CancellationTokenSource cancellationTokenSource;

        internal CancellationToken CancellationToken => cancellationTokenSource.Token;

        internal async Task ProcessUpdate(Update update)
        {
            try
            {
                await StartProcess(update);
            }
            catch (Exception ex)
            {
                await ProcessError(ex, update);
            }
        }

        private async Task StartProcess(Update update)
        {
            HandlerResult handlerResult = new HandlerResult()
            {
                Update = update,
                HasAccess = true, // Устанавливаем true, считая любое первоначальное обращение разрешённым (чтобы первый раз пройти условие на .HasAccess) 
            };

            foreach (CoreHandler handler in Handlers)
            {
                if (handlerResult.HasAccess)
                {
                    handlerResult = handler.Process(handlerResult);

                    // Перепроверяем доступ, т.к. во время работы очередного обработчика доступ мог быть отозван
                    if (handlerResult.InformationSet != null && handlerResult.HasAccess)
                    {
                        await SendResponse(handlerResult.ShortUpdateInfo.ChatId, handlerResult.InformationSet, handlerResult.CurrentUserContext);

                        if (!handlerResult.IsNeedContinue)
                            return;
                    }
                }
            }
        }

        private async Task ProcessError(Exception ex, Update update)
        {
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            EventId eventId = EventIDHelper.GetNextEventIdThreadSave(CommonConsts.EventNames.RuntimeError);

            await LogRuntimeError(updateInfo, eventId, ex);

            if (ConfigurationData.Notifications.NotifyOwnersAboutRuntimeErrors)
                await BotResponseSender.SendSimpleMassiveResponse(ConfigurationData.Bot.OwnersChatIDs, @$"Ошибка во время исполнения. EventID: {eventId.Id}
{ex.ToString()}");

        }

        private async Task LogRuntimeError(IUpdateInfo updateInfo, EventId eventId, Exception ex)
        {
            if (updateInfo is ShortUpdateInfo shortUpdateInfo)
            {
                long? userId = shortUpdateInfo.User?.Id;

                logger.Log(LogLevel.Error,
                           eventId,
                           new Dictionary<string, object>()
                           {
                               { "TelegaramUserId", userId },
                           },
                           ex,
                           LogFormatter.EmptyFormatter);

                if (shortUpdateInfo.ChatId > 0)
                {
                    // не отправлять EventID, если нет доступа у пользователя
                    // выглядит излишне, но пускай будет
                    // if(IsNeedSendEventIdToUser(userId))

                    await BotResponseSender.SimpleNotification(shortUpdateInfo.ChatId, $"Ошибка обработки. EventId: {eventId.Id}");
                }
            }
            else
                logger.Log(LogLevel.Error, 
                           eventId,
                           new Dictionary<string, object>(),
                           ex,
                           LogFormatter.EmptyFormatter);
        }

        private bool IsNeedSendEventIdToUser(long? userId)
        {
            if (userId == null)
                return false;

            AdminRepository adminRepository = GetRequiredRepository<AdminRepository>();

            UserInformation? userInformation = adminRepository.GetUserInformation(userId.Value);

            return userInformation != null && adminRepository.UserHasAccess(userInformation);
        }

        private async Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext)
            => await BotResponseSender.SendResponse(chatId, messageInformationSetting, currentUserContext);

        internal T GetRequiredHandler<T>() where T : CoreHandler
        {
            T? handler = GetHandler<T>();

            if (handler == null)
            
                throw new InvalidOperationException($"Обработчик {typeof(T).Name} не найден");

            return handler;
        }

        internal T? GetHandler<T>() where T : CoreHandler
            => Handlers.OfType<T>().FirstOrDefault();

        internal T GetRequiredRepository<T>() where T : CoreRepository
        {
            T? repository = GetRepository<T>();

            if (repository == null)
                throw new InvalidOperationException($"Репозиторий {typeof(T).Name} не найден");

            return repository;
        }

        internal T? GetRepository<T>() where T : CoreRepository
            => Repositories.OfType<T>().FirstOrDefault();

        internal async Task StopManaging(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 2)
                timeSpan = TimeSpan.FromSeconds(2);

            logger.LogWarning("Инициировано отключение бота");

            await Task.Delay(timeSpan);

            this.cancellationTokenSource.Cancel();
        }
    }
}