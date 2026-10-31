using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.BusinessLogic.Helpers.Updates;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Consts;
using WorkoutStorageBot.Core.Handlers.Abstraction;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;
using WorkoutStorageBot.Model.DTO.Log;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.Core.Manager
{
    internal class CoreManager
    {
        internal CoreManager(CoreTools coreTools)
        {
            this.coreTools = coreTools;

            this.repositoriesStore = new RepositoriesStore(this.coreTools.Db);

            this.handlers = new List<CoreHandler>()
            {
                new PrimaryUpdateHandler(this.coreTools, this.repositoriesStore),
                new UpdateHandler(this.coreTools, this.repositoriesStore),
            };

            this.logger = this.coreTools.LoggerFactory.CreateLogger<CoreManager>();
        }

        private readonly CoreTools coreTools;

        private readonly RepositoriesStore repositoriesStore;

        private readonly List<CoreHandler> handlers;

        private readonly ILogger logger;

        private IBotResponseSender BotResponseSender => coreTools.BotResponseSender;

        internal async Task ProcessUpdate(Update update, Action<TimeSpan> stopAppMethod, CancellationToken cancellationToken)
        {
            try
            {
                await StartProcess(update, stopAppMethod, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogWarning(ex, "Получен OperationCanceledException");
            }
            catch (Exception ex)
            {
                await ProcessError(ex, update, cancellationToken);
            }
        }

        private async Task StartProcess(Update update, Action<TimeSpan> stopAppMethod, CancellationToken cancellationToken)
        {
            HandlerResult handlerResult = new HandlerResult()
            {
                Update = update,
                HasAccess = true, // Устанавливаем true, считая любое первоначальное обращение разрешённым (чтобы первый раз пройти условие на .HasAccess) 
            };

            foreach (CoreHandler handler in handlers)
            {
                if (handlerResult.HasAccess)
                {
                    handlerResult = await handler.Process(handlerResult, cancellationToken);

                    if (handlerResult.IsNeedStopBot)
                        StopApp(TimeSpan.FromSeconds(2), stopAppMethod);

                    // Перепроверяем доступ, т.к. во время работы очередного обработчика доступ мог быть отозван
                    if (handlerResult is AuthorizedHandlerResult authorizedHandlerResult && authorizedHandlerResult.InformationSet != null && handlerResult.HasAccess)
                    {
                        await BotResponseSender.SendResponse(authorizedHandlerResult.ShortUpdateInfo.ChatId, authorizedHandlerResult.InformationSet, authorizedHandlerResult.CurrentUserContext, cancellationToken);

                        if (!handlerResult.IsNeedContinue)
                            return;
                    }
                }
            }
        }

        private async Task ProcessError(Exception ex, Update update, CancellationToken cancellationToken)
        {
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(update);

            EventId eventId = EventIDHelper.GetNextEventIdThreadSafe(CommonConsts.EventNames.RuntimeError);

            await LogRuntimeError(updateInfo, eventId, ex, cancellationToken);

            if (this.coreTools.ConfigurationData.Notifications.NotifyOwnersAboutRuntimeErrors)
                await NotifyOwnersAboutError(ex, eventId, cancellationToken);
        }

        private async Task LogRuntimeError(IUpdateInfo updateInfo, EventId eventId, Exception ex, CancellationToken cancellationToken)
        {
            if (updateInfo is ShortUpdateInfo shortUpdateInfo)
            {
                long userId = shortUpdateInfo.User.Id;

                logger.Log(LogLevel.Error,
                           eventId,
                           new LogData() { TelegramUserId = userId },
                           ex,
                           LogFormatter.SimpleFormatter);

                // не отправлять EventID, если нет доступа у пользователя
                if (shortUpdateInfo.ChatId > 0 && await IsNeedSendEventIdToUser(userId, cancellationToken))
                    await BotResponseSender.SendSimpleNotification(shortUpdateInfo.ChatId, $"Ошибка обработки. EventId: {eventId.Id}", cancellationToken);
            }
            else
                logger.LogError(eventId, ex, null);
        }

        private async Task<bool> IsNeedSendEventIdToUser(long userId, CancellationToken cancellationToken)
        {
            AdminWrapper adminWrapper = this.repositoriesStore.InitRepository(x => new AdminWrapper(x, coreTools.ConfigurationData, coreTools.LoggerFactory));

            UserInformation? userInformation = await adminWrapper.GetUserInformationWithoutTracking(userId, cancellationToken);

            return userInformation != null && adminWrapper.UserHasAccess(userInformation);
        }

        private async Task NotifyOwnersAboutError(Exception ex, EventId eventId, CancellationToken cancellationToken)
        {
            string exMessage = ex.ToString();

            if (exMessage.Length > CoreConsts.Log.ShowLimit)
                exMessage = $"{exMessage.Substring(0, CoreConsts.Log.ShowLimit)}...";

            await this.BotResponseSender.SendSimpleMassiveNotification(this.coreTools.ConfigurationData.Bot.OwnersChatIDs, 
                                                                       @$"Ошибка во время исполнения. EventID: {eventId.Id}
======================
{exMessage}",
                                                                       cancellationToken);    
        }

        private void StopApp(TimeSpan timeSpan, Action<TimeSpan> stopAppMethod)
        {
            if (timeSpan.TotalSeconds < 2)
                timeSpan = TimeSpan.FromSeconds(2);

            logger.LogWarning("Инициировано отключение бота");

            stopAppMethod.Invoke(timeSpan);
        }
    }
}