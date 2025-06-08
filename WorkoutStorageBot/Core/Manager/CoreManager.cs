#region using

using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WorkoutStorageBot.Application.BotTools.Logging;
using WorkoutStorageBot.Application.BotTools.Sender;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.Updates;
using WorkoutStorageBot.Model.Domain;
using WorkoutStorageBot.Model.HandlerData.Results;
using WorkoutStorageBot.Model.HandlerData.Results.UpdateInfo;

#endregion using

namespace WorkoutStorageBot.Core.Manager
{
    internal class CoreManager
    {
        internal CoreManager(List<CoreHandler> handlers,
                             List<CoreRepository> repositories,
                             ConfigurationData configurationData,
                             IBotResponseSender botResponseSender,
                             ILoggerFactory loggerFactory,
                             CancellationTokenSource cancellationToken)
        {
            Handlers = CommonHelper.GetIfNotNull(handlers);

            Repositories = CommonHelper.GetIfNotNull(repositories);

            ConfigurationData = CommonHelper.GetIfNotNull(configurationData);

            _loggerFactory = CommonHelper.GetIfNotNull(loggerFactory);

            _logger = _loggerFactory.CreateLogger<CoreManager>();

            BotResponseSender = CommonHelper.GetIfNotNull(botResponseSender);

            _cancellationTokenSource = CommonHelper.GetIfNotNull(cancellationToken);

            ResetCoreManager();
        }

        internal List<CoreHandler> Handlers { get; }
        internal List<CoreRepository> Repositories { get; }

        private ConfigurationData ConfigurationData { get; }

        private IBotResponseSender BotResponseSender { get; }

        private readonly ILoggerFactory _loggerFactory;

        private readonly ILogger<CoreManager> _logger;

        private readonly CancellationTokenSource _cancellationTokenSource;

        internal CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private static readonly SemaphoreSlim processSemaphore = new SemaphoreSlim(1, 1);

        internal async Task ProcessUpdate(Update update)
        {
            await processSemaphore.WaitAsync(); // входим в критическую секцию

            try
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
            catch (Exception ex)
            {
                EventId eventId = EventIDHelper.GetNextEventId(CommonConsts.EventNames.RuntimeError);

                IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

                if (updateInfo is ShortUpdateInfo shortUpdateInfo)
                {
                    long? userId = shortUpdateInfo.User?.Id;

                    _logger.Log(LogLevel.Error,
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

                if (ConfigurationData.Notifications.NotifyOwnersAboutRuntimeErrors)
                    await BotResponseSender.SendSimpleMassiveResponse(ConfigurationData.Bot.OwnersChatIDs, @$"Ошибка во время исполнения. EventID: {eventId.Id}
{ex.ToString()}");

            }
            finally
            {
                processSemaphore.Release(); // освобождаем секцию
            }
        }

        private bool IsNeedSendEventIdToUser(long? userId)
        {
            if (userId == null)
                return false;

            AdminRepository adminRepository = GetRepository<AdminRepository>(false);

            if (adminRepository == null)
                return false;

            UserInformation? userInformation = adminRepository.GetUserInformation(userId.Value);

            return userInformation != null && adminRepository.UserHasAccess(userInformation);
        }

        private void ResetCoreManager()
        {
            foreach (CoreHandler handler in Handlers)
            {
                handler.TryResetCoreManager(this);
            }

            foreach (CoreRepository repository in Repositories)
            {
                repository.TryResetCoreManager(this);
            }
        }

        private async Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext)
            => await BotResponseSender.SendResponse(chatId, messageInformationSetting, currentUserContext);

        internal T? GetHandler<T>(bool throwNotFoundEx = true) where T : CoreHandler
        {
            T? handler = Handlers.OfType<T>().FirstOrDefault();

            if (handler == null)
            {
                if (throwNotFoundEx)
                    throw new InvalidOperationException($"Обработчик {typeof(T).Name} не найден");
                else
                    return default;
            }

            return handler;
        }

        internal T? GetRepository<T>(bool throwNotFoundEx = true) where T : CoreRepository
        {
            T? repository = Repositories.OfType<T>().FirstOrDefault();

            if (repository == null)
            {
                if (throwNotFoundEx)
                    throw new InvalidOperationException($"Репозиторий {typeof(T).Name} не найден");
                else
                    return default;
            }

            return repository;
        }

        internal async Task StopManaging(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 2)
                timeSpan = TimeSpan.FromSeconds(2);

            _logger.LogWarning("Инициировано отключение бота");

            await Task.Delay(timeSpan);

            this._cancellationTokenSource.Cancel();
        }
    }
}