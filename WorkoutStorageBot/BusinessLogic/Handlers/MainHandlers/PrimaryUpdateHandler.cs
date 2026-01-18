using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Context.StepStore;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Helpers.Updates;

namespace WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers
{
    internal class PrimaryUpdateHandler : CoreHandler
    {
        private IContextKeeper ContextKeeper { get; }

        private AdminRepository AdminRepository { get; }

        protected override ILogger Logger { get; }

        internal PrimaryUpdateHandler(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(PrimaryUpdateHandler))
        {
            this.Logger = CoreTools.LoggerFactory.CreateLogger<PrimaryUpdateHandler>();

            this.ContextKeeper = CoreManager.ContextKeeper;

            this.AdminRepository = CoreManager.GetRequiredRepository<AdminRepository>();
        }

        internal override async Task<HandlerResult> Process(HandlerResult handlerResult)
        {
            IUpdateInfo updateInfo = UpdatesHelper.GetUpdateInfo(handlerResult.Update);

            if (updateInfo is UnknownUpdateInfo unknownUpdateInfo)
            {
                ProcessUnknownUpdateType(unknownUpdateInfo);
                return handlerResult;
            }
            else if (updateInfo is ShortUpdateInfo shortUpdateInfo)
            {
                (UserContext? userContext, bool isNewContext) = await GetOrCreateContext(shortUpdateInfo.User);

                if (userContext == null)
                    return handlerResult;

                PrimaryHandlerResult primaryHandlerResult = CreatePrimaryHandlerResult(shortUpdateInfo, userContext, isNewContext);

                if (primaryHandlerResult.ShortUpdateInfo.IsExpectedType)
                    ProcessExpectedUpdateType(primaryHandlerResult);
                else
                    ProcessUnexpectedUpdateType(primaryHandlerResult);

                return primaryHandlerResult;
            }
            else
                throw new InvalidOperationException($"Неожиданный тип IUpdateInfo: {updateInfo.GetType().Name}");
        }

        private void ProcessUnknownUpdateType(UnknownUpdateInfo unknownUpdateInfo)
            => Logger.LogWarning($"Получен неизвестный update c типом {unknownUpdateInfo.UpdateType.ToString()}");

        private async Task<(UserContext? userContext, bool isNewContext)> GetOrCreateContext(User user)
        {
            UserContext? userContext = GetExistingContext(user);

            if (userContext == null)
                return (await CreateNewContext(user), true);

            if (AdminRepository.UserHasAccess(userContext.UserInformation))
                return (userContext, false);

            return (null, false);
        }

        private UserContext? GetExistingContext(User user)
            => ContextKeeper.HasContext(user.Id)
                ? ContextKeeper.GetContext(user.Id)
                : null;

        private async Task<UserContext?> CreateNewContext(User user)
        {
            UserInformation? userInformation = await GetOrCreateNewUserInformation(user);

            if (userInformation == null || !AdminRepository.UserHasAccess(userInformation))
                return null;

            Roles currentRoles = GetUserRoles(userInformation, AdminRepository);

            DTOUserInformation DTOCurrentUser = EntityConverter.ToDTOUserInformation(userInformation);

            UserContext userContext = new UserContext(DTOCurrentUser, currentRoles, CoreTools.ConfigurationData.Bot.IsNeedLimits);

            ContextKeeper.AddContext(user.Id, userContext);

            return userContext;
        }

        private async Task<UserInformation?> GetOrCreateNewUserInformation(User user)
            => await AdminRepository.GetFullUserInformationWithoutTracking(user.Id) 
                ?? await AdminRepository.TryCreateNewUserInformation(user);

        private Roles GetUserRoles(UserInformation currentUser, AdminRepository adminRepository)
        {
            Roles currentRoles = adminRepository.UserIsOwner(currentUser.UserId)
                    ? Roles.Admin
                    : Roles.User;

            return currentRoles;
        }

        private PrimaryHandlerResult CreatePrimaryHandlerResult(ShortUpdateInfo shortUpdateInfo, UserContext userContext, bool isNewContext)
        {
            return new PrimaryHandlerResult()
            {
                Update = shortUpdateInfo.Update,
                ShortUpdateInfo = shortUpdateInfo,
                CurrentUserContext = userContext,
                HasAccess = true,
                IsNewContext = isNewContext
            };
        }

        private void ProcessExpectedUpdateType(PrimaryHandlerResult primaryHandledData)
        {
            LoggingExpectedUpdateType(primaryHandledData.ShortUpdateInfo.User, primaryHandledData.ShortUpdateInfo.Data, primaryHandledData.ShortUpdateInfo.UpdateType);

            if (primaryHandledData.IsNewContext)
                SetInformationSetForNewContext(primaryHandledData);
            else 
                primaryHandledData.IsNeedContinue = true;
        }

        private void ProcessUnexpectedUpdateType(PrimaryHandlerResult primaryHandledData)
        {
            ShortUpdateInfo shortUpdateInfo = primaryHandledData.ShortUpdateInfo;

            string textUpdateType;

            if (shortUpdateInfo.UpdateType == UpdateType.Message)
                textUpdateType = shortUpdateInfo.Data;
            else
                textUpdateType = shortUpdateInfo.UpdateType.ToString();

            string message = $"Неподдерживаемый тип сообщения: {textUpdateType}";

            EventId eventId = EventIDHelper.GetNextEventIdThreadSafe(CommonConsts.EventNames.NotSupportedUpdateType);

            primaryHandledData.InformationSet = new MessageInformationSet($"{message} | EventId: {eventId.Id}");
            primaryHandledData.IsNeedContinue = false;

            Logger.Log(LogLevel.Information,
                       eventId,
                       new Dictionary<string, object>()
                       {
                           { "Message", message },
                           { "TelegramUserId", shortUpdateInfo.User.Id },
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

            EventId eventId = EventIDHelper.GetNextEventIdThreadSafe(CommonConsts.EventNames.ExpectedUpdateType);

            Logger.Log(logLevel,
                       eventId,
                       new Dictionary<string, object>()
                       {
                           { "Message", message },
                           { "TelegramUserId", user.Id },
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
                primaryHandledData.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.Start);
                message = "Начнём";
                buttonsSet = ButtonsSet.AddCycle;
            }

            primaryHandledData.InformationSet = new MessageInformationSet(message, (buttonsSet, ButtonsSet.None));
            primaryHandledData.IsNeedContinue = false;
        }
    }
}