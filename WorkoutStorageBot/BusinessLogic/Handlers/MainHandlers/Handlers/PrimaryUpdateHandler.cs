#region using

using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.GlobalContext;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.StepStore;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.EventIDHelper;
using WorkoutStorageBot.Helpers.Updates;
using WorkoutStorageBot.Model.Domain;
using WorkoutStorageBot.Model.HandlerData;
using WorkoutStorageBot.Model.HandlerData.Results;
using WorkoutStorageBot.Model.HandlerData.Results.UpdateInfo;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers.Handlers
{
    internal class PrimaryUpdateHandler : CoreHandler
    {
        private ContextStore ContextStore { get; }

        protected override ILogger Logger { get; }

        internal PrimaryUpdateHandler(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(PrimaryUpdateHandler))
        {
            Logger = CoreTools.LoggerFactory.CreateLogger<PrimaryUpdateHandler>();

            ContextStore = new ContextStore(CommonHelper.GetIfNotNull(coreTools.ConfigurationData.Bot.IsNeedCacheContext));
        }

        internal PrimaryUpdateHandler(CoreTools coreTools) : base(coreTools, nameof(PrimaryUpdateHandler))
        {
            Logger = CoreTools.LoggerFactory.CreateLogger<PrimaryUpdateHandler>();

            ContextStore = new ContextStore(CommonHelper.GetIfNotNull(coreTools.ConfigurationData.Bot.IsNeedCacheContext));
        }

        internal override PrimaryHandlerResult Process(HandlerResult handlerResult)
        {
            PrimaryHandlerResult primaryHandlerResult = InitHandlerResult(handlerResult);
            
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(handlerResult.Update);

            if (updateInfo is UnknownUpdateInfo unknownUpdateInfo)
            {
                ProcessUnknownUpdateType(unknownUpdateInfo);
            }
            else if (updateInfo is ShortUpdateInfo shortUpdateInfo)
            {
                primaryHandlerResult.ShortUpdateInfo = shortUpdateInfo;

                TryGetContextAndAccess(primaryHandlerResult);

                if (primaryHandlerResult.ShortUpdateInfo.IsExpectedType)
                    ProcessExpectedUpdateType(primaryHandlerResult);
                else
                    ProcessUnexpectedUpdateType(primaryHandlerResult);
            }

            return primaryHandlerResult;
        }

        protected override PrimaryHandlerResult InitHandlerResult(HandlerResult handlerResult)
        {
            ValidateHandlerResult(handlerResult);

            return new PrimaryHandlerResult()
            {
                Update = handlerResult.Update,
            };
        }

        protected override void ValidateHandlerResult(HandlerResult handlerResult)
                => base.ValidateHandlerResult(handlerResult);

        private void ProcessUnknownUpdateType(UnknownUpdateInfo unknownUpdateInfo)
            =>Logger.LogWarning($"Получен неизвестный update c типом {unknownUpdateInfo.UpdateType.ToString()}");

        private void TryGetContextAndAccess(PrimaryHandlerResult primaryHandledData)
        {
            AdminRepository adminRepository = CoreManager.GetRepository<AdminRepository>();

            User user = primaryHandledData.ShortUpdateInfo.User;

            if (!ContextStore.HasContext(user.Id))
            {
                AddNewContext(primaryHandledData, adminRepository);
                primaryHandledData.IsNewContext = true;
            }
            else
            {
                primaryHandledData.CurrentUserContext = ContextStore.GetContext(user.Id) 
                    ?? throw new InvalidOperationException($"Аномалия: Не удалось найти userContext с userID: '{user.Id}'");
                primaryHandledData.HasAccess = adminRepository.UserHasAccess(primaryHandledData.CurrentUserContext.UserInformation);
                primaryHandledData.IsNewContext = false;
            }
        }

        private void AddNewContext(PrimaryHandlerResult primaryHandledData, AdminRepository adminRepository)
        {
            UserInformation currentUser = adminRepository.GetFullUserInformation(primaryHandledData.ShortUpdateInfo.User.Id)
                ?? adminRepository.CreateNewUser(primaryHandledData.ShortUpdateInfo.User);

            primaryHandledData.HasAccess = adminRepository.UserHasAccess(currentUser);

            if (primaryHandledData.HasAccess)
            {
                Roles currentRoles = GetUserRoles(currentUser, adminRepository);

                primaryHandledData.CurrentUserContext = new UserContext(currentUser, currentRoles, CoreTools.ConfigurationData.Bot.IsNeedLimits);
                ContextStore.AddContext(primaryHandledData.ShortUpdateInfo.User.Id, primaryHandledData.CurrentUserContext);
            }
        }

        private Roles GetUserRoles(UserInformation currentUser, AdminRepository adminRepository)
        {
            Roles currentRoles = adminRepository.UserIsOwner(currentUser)
                    ? Roles.Admin
                    : Roles.User;

            return currentRoles;
        }

        private void ProcessExpectedUpdateType(PrimaryHandlerResult primaryHandledData)
        {
            LoggingExpectedUpdateType(primaryHandledData.ShortUpdateInfo.User, primaryHandledData.ShortUpdateInfo.Data, primaryHandledData.ShortUpdateInfo.UpdateType);

            if (primaryHandledData.IsNewContext && primaryHandledData.HasAccess)
                SetInformationSetForNewContext(primaryHandledData);
            else 
                primaryHandledData.IsNeedContinue = true;
        }

        private void ProcessUnexpectedUpdateType(PrimaryHandlerResult primaryHandledData)
        {
            ShortUpdateInfo shortUpdateInfo = primaryHandledData.ShortUpdateInfo;

            string textUpdateType;

            if (shortUpdateInfo.UpdateType == UpdateType.Message)
                textUpdateType = primaryHandledData.Update.Message.Type.ToString();
            else
                textUpdateType = shortUpdateInfo.UpdateType.ToString();

            string message = $"Неподдерживаемый тип сообщения: {textUpdateType}";

            EventId eventId = EventIDHelper.GetNextEventId(CommonConsts.EventNames.NotSupportedUpdateType);

            primaryHandledData.InformationSet = new MessageInformationSet($"{message} | EventId: {eventId.Id}");
            primaryHandledData.IsNeedContinue = false;

            Logger.Log(LogLevel.Information,
                       eventId,
                       new Dictionary<string, object>()
                       {
                           { "Message", message },
                           { "TelegaramUserId", shortUpdateInfo.User.Id },
                       },
                       null,
                       LogFormatter.EmptyFormatter);
        }

        private void LoggingExpectedUpdateType(User user, string data, UpdateType updateType)
        {
            LogLevel logLevel = LogLevel.Information;
            string message = string.Empty;

            switch (updateType)
            {
                case UpdateType.Message:
                    message = $"Text: {data}";

                    break;

                case UpdateType.CallbackQuery:
                    message = $"CallbackQuery: {data}";

                    break;
                default:
                    message = $"Неожиданный updateType: {updateType}";
                    logLevel = LogLevel.Warning;

                    break;
            }

            EventId eventId = EventIDHelper.GetNextEventId(CommonConsts.EventNames.ExpectedUpdateType);

            Logger.Log(logLevel,
                       eventId,
                       new Dictionary<string, object>()
                       {
                           { "Message", message },
                           { "TelegaramUserId", user.Id },
                       },
                       null,
                       LogFormatter.EmptyFormatter);
        }

        private void SetInformationSetForNewContext(PrimaryHandlerResult primaryHandledData)
        {
            bool hasCycle;
            hasCycle = primaryHandledData.CurrentUserContext.ActiveCycle != null ? true : false;

            string message;
            ButtonsSet buttonsSet;

            if (hasCycle)
            {
                StepInformation mainStep = StepStorage.GetMainStep();
                message = new ResponseTextConverter("Информация о предыдущей сессии не была найдена", mainStep.Message).Convert();
                buttonsSet = mainStep.ButtonsSet;
            }
            else
            {
                primaryHandledData.CurrentUserContext.Navigation.QueryFrom = QueryFrom.Start;
                message = "Начнём";
                buttonsSet = ButtonsSet.AddCycle;
            }

            primaryHandledData.InformationSet = new MessageInformationSet(message, (buttonsSet, ButtonsSet.None));

            // Не обязательно. Чтобы не было анимации "зависание кнопки" в ТГ боте
            if (primaryHandledData.ShortUpdateInfo.UpdateType == UpdateType.CallbackQuery)
                primaryHandledData.InformationSet.AdditionalParameters.Add("BotCallBackID", primaryHandledData.Update.CallbackQuery.Id);

            primaryHandledData.IsNeedContinue = false;
        }

        internal void DeleteContext(long userID)
        {
            ContextStore.RemoveContext(userID);
        }
    }
}