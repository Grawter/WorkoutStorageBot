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
using WorkoutStorageBot.Model.Entities.BusinessLogic;
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

            this.ContextKeeper = coreManager.ContextKeeper;

            this.AdminRepository = CoreManager.GetRequiredRepository<AdminRepository>();
        }

        internal override async Task<HandlerResult> Process(HandlerResult handlerResult)
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

                await TryGetContextAndAccess(primaryHandlerResult);

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

        private async Task TryGetContextAndAccess(PrimaryHandlerResult primaryHandledData)
        {
            User user = primaryHandledData.ShortUpdateInfo.User;

            if (!ContextKeeper.HasContext(user.Id))
                await AddNewContext(primaryHandledData);
            else
                SetExistingContext(primaryHandledData, user.Id);
        }

        private async Task AddNewContext(PrimaryHandlerResult primaryHandledData)
        {
            UserInformation? currentUser = await AdminRepository.GetFullUserInformationWithoutTracking(primaryHandledData.ShortUpdateInfo.User.Id)
                ?? await AdminRepository.CreateNewUser(primaryHandledData.ShortUpdateInfo.User);

            primaryHandledData.HasAccess = AdminRepository.UserHasAccess(currentUser);

            if (primaryHandledData.HasAccess)
            {
                Roles currentRoles = GetUserRoles(currentUser, AdminRepository);

                DTOUserInformation DTOCurrentUser = EntityConverter.ToDTOUserInformation(currentUser);

                primaryHandledData.CurrentUserContext = new UserContext(DTOCurrentUser, currentRoles, CoreTools.ConfigurationData.Bot.IsNeedLimits);
                ContextKeeper.AddContext(primaryHandledData.ShortUpdateInfo.User.Id, primaryHandledData.CurrentUserContext);
            }

            primaryHandledData.IsNewContext = true;
        }

        private void SetExistingContext(PrimaryHandlerResult primaryHandledData, long userID)
        {
            primaryHandledData.CurrentUserContext = ContextKeeper.GetContext(userID)
                    ?? throw new InvalidOperationException($"Аномалия: Не удалось найти userContext с userID: '{userID}'");
            primaryHandledData.HasAccess = AdminRepository.UserHasAccess(primaryHandledData.CurrentUserContext.UserInformation);
            primaryHandledData.IsNewContext = false;
        }

        private Roles GetUserRoles(UserInformation currentUser, AdminRepository adminRepository)
        {
            Roles currentRoles = adminRepository.UserIsOwner(currentUser.UserId)
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

            EventId eventId = EventIDHelper.GetNextEventIdThreadSave(CommonConsts.EventNames.NotSupportedUpdateType);

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

            EventId eventId = EventIDHelper.GetNextEventIdThreadSave(CommonConsts.EventNames.ExpectedUpdateType);

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
                primaryHandledData.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.Start);
                message = "Начнём";
                buttonsSet = ButtonsSet.AddCycle;
            }

            primaryHandledData.InformationSet = new MessageInformationSet(message, (buttonsSet, ButtonsSet.None));
            primaryHandledData.IsNeedContinue = false;
        }
    }
}