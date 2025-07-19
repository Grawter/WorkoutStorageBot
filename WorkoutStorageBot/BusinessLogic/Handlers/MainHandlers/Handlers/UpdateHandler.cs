#region using

using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.HandlerData;
using WorkoutStorageBot.Model.HandlerData.Results;
using WorkoutStorageBot.Model.HandlerData.Results.UpdateInfo;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers.Handlers
{
    internal class UpdateHandler : CoreHandler
    {
        private UserContext CurrentUserContext { get; set; }

        internal UpdateHandler(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(PrimaryUpdateHandler))
        { }

        internal UpdateHandler(CoreTools coreTools) : base(coreTools, nameof(PrimaryUpdateHandler))
        { }

        internal override UpdateHandlerResult Process(HandlerResult handlerResult)
        {
            UpdateHandlerResult updateHandlerResult = InitHandlerResult(handlerResult);
            CurrentUserContext = updateHandlerResult.CurrentUserContext;

            switch (updateHandlerResult.ShortUpdateInfo.UpdateType)
            {
                case UpdateType.Message:
                    updateHandlerResult.InformationSet = ProcessMessage(updateHandlerResult.ShortUpdateInfo);
                    break;
                case UpdateType.CallbackQuery:
                    updateHandlerResult.InformationSet = ProcessCallbackQuery(updateHandlerResult.ShortUpdateInfo);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный update.type: {updateHandlerResult.ShortUpdateInfo.UpdateType}");
            }

            return updateHandlerResult;
        }

        protected override UpdateHandlerResult InitHandlerResult(HandlerResult handlerResult)
        {
            ValidateHandlerResult(handlerResult);

            return new UpdateHandlerResult()
            {
                Update = handlerResult.Update,
                ShortUpdateInfo = handlerResult.ShortUpdateInfo,
                CurrentUserContext = handlerResult.CurrentUserContext,
                HasAccess = handlerResult.HasAccess,
            };
        }

        protected override void ValidateHandlerResult(HandlerResult handlerResult)
        {
            base.ValidateHandlerResult(handlerResult);

            ArgumentNullException.ThrowIfNull(handlerResult.ShortUpdateInfo);
            ArgumentNullException.ThrowIfNull(handlerResult.CurrentUserContext);
        }

        private IInformationSet ProcessMessage(ShortUpdateInfo updateInfo)
        {
            CommandHandlerData commandHandlerData = new CommandHandlerData()
            {
                Db = CoreTools.Db,
                ParentHandler = this,
                CurrentUserContext = CurrentUserContext,
            };

            TextMessageConverter requestConverter = new TextMessageConverter(updateInfo.Data);

            TextMessageCH commandHandler = new TextMessageCH(commandHandlerData, requestConverter);

            TextMessageCH CHResult;

            switch (CurrentUserContext.Navigation.MessageNavigationTarget)
            {
                case MessageNavigationTarget.Default:
                    CHResult = commandHandler.DefaultCommand();

                    break;

                case MessageNavigationTarget.AddCycle:
                    CHResult = commandHandler.Expectation(HandlerAction.Add, HandlerAction.Save)
                                             .AddCycleCommand();

                    break;

                case MessageNavigationTarget.AddDays:
                    CHResult = commandHandler.Expectation(HandlerAction.Add, HandlerAction.Save)
                                             .AddDaysCommand();

                    break;

                case MessageNavigationTarget.AddExercises:
                    CHResult = commandHandler.AddExercisesCommand();

                    break;

                case MessageNavigationTarget.AddResultForExercise:
                    CHResult = commandHandler.AddResultForExerciseCommand();

                    break;

                case MessageNavigationTarget.ChangeNameCycle:
                    CHResult = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                             .ChangeNameCommand(CommonConsts.Domain.Cycle);

                    break;

                case MessageNavigationTarget.ChangeNameDay:
                    CHResult = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                             .ChangeNameCommand(CommonConsts.Domain.Day);

                    break;

                case MessageNavigationTarget.ChangeNameExercise:
                    CHResult = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                             .ChangeNameCommand(CommonConsts.Domain.Exercise);

                    break;

                case MessageNavigationTarget.FindLogByID:
                    CHResult = commandHandler.FindLogByIDCommand(isEventID: false);

                    break;

                case MessageNavigationTarget.FindLogByEventID:
                    CHResult = commandHandler.FindLogByIDCommand(isEventID: true);

                    break;

                case MessageNavigationTarget.ChangeUserState:
                    CHResult = commandHandler.ChangeUserStateCommand();

                    break;

                case MessageNavigationTarget.DeleteUser:
                    CHResult = commandHandler.DeleteUserCommand();

                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.MessageNavigationTarget: {CurrentUserContext.Navigation.MessageNavigationTarget}!");
            }

            IInformationSet informationSet = CHResult.GetData();

            return informationSet;
        }

        private IInformationSet ProcessCallbackQuery(ShortUpdateInfo updateInfo)
        {
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser(updateInfo.Data);

            if (CheckingComplianceCallBackId(callbackQueryParser.CallBackId, out IInformationSet informationSet))
            {
                CommandHandlerData commandHandlerData = new CommandHandlerData()
                {
                    Db = CoreTools.Db,
                    ParentHandler = this,
                    CurrentUserContext = CurrentUserContext,
                };

                switch ((CallBackNavigationTarget)callbackQueryParser.Direction)
                {
                    case CallBackNavigationTarget.None:
                        informationSet = ProcessCommonCallBack(commandHandlerData, callbackQueryParser);
                        break;
                    case CallBackNavigationTarget.Workout:
                        informationSet = ProcessWorkoutCallBack(commandHandlerData, callbackQueryParser);
                        break;
                    case CallBackNavigationTarget.Settings:
                        informationSet = ProcessSettingsCallBack(commandHandlerData, callbackQueryParser);
                        break;
                    case CallBackNavigationTarget.Admin:
                        informationSet = ProcessAdminCallBack(commandHandlerData, callbackQueryParser);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный CallBackNavigationTarget: {(CallBackNavigationTarget)callbackQueryParser.Direction}");
                }
            }

            // Не обязательно. Чтобы не было анимации "зависание кнопки" в ТГ боте
            informationSet.AdditionalParameters.Add("BotCallBackID", updateInfo.Update.CallbackQuery.Id);

            return informationSet;
        }

        private IInformationSet ProcessCommonCallBack(CommandHandlerData commandHandlerData, CallbackQueryParser callbackQueryParser)
        {
            CommonCH commandHandler = new CommonCH(commandHandlerData, callbackQueryParser);

            CommonCH CHResult;

            switch (callbackQueryParser.SubDirection)
            {
                case "Back":
                    CHResult = commandHandler.BackCommand();
                    break;

                case "ToMain":
                    CHResult = commandHandler.ToMainCommand();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            IInformationSet informationSet = CHResult.GetData();

            return informationSet;
        }

        private IInformationSet ProcessWorkoutCallBack(CommandHandlerData commandHandlerData, CallbackQueryParser callbackQueryParser)
        {
            WorkoutCH commandHandler = new WorkoutCH(commandHandlerData, callbackQueryParser);

            WorkoutCH CHResult;

            switch (callbackQueryParser.SubDirection)
            {
                case "Workout":
                    CHResult = commandHandler.WorkoutCommand();
                    break;

                case "LastResult":
                    CHResult = commandHandler.LastResultCommand();
                    break;

                case "SaveResultsExercise":
                    CHResult = commandHandler.Expectation(HandlerAction.Save)
                                             .SaveResultsExerciseCommand();
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            IInformationSet informationSet = CHResult.GetData();

            return informationSet;
        }

        private IInformationSet ProcessSettingsCallBack(CommandHandlerData commandHandlerData, CallbackQueryParser callbackQueryParser)
        {
            SettingsCH commandHandler = new SettingsCH(commandHandlerData, callbackQueryParser);

            SettingsCH CHResult;

            switch (callbackQueryParser.SubDirection)
            {
                case "Settings":
                    CHResult = commandHandler.SettingsCommand();
                    break;

                case "ArchiveStore":
                    CHResult = commandHandler.ArchiveStoreCommand();
                    break;

                case "Archive":
                    CHResult = commandHandler.ArchiveCommand();
                    break;
                case "Export":
                    CHResult = commandHandler.ExportCommand();
                    break;

                case "ExportTo":
                    CHResult = commandHandler.ExportToCommand();
                    break;

                case "UnArchive":
                    CHResult = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                             .UnArchiveCommand();
                    break;

                case "Setting":
                    CHResult = commandHandler.SettingCommand();
                    break;

                case "Add":
                    CHResult = commandHandler.AddCommand();
                    break;

                case "SaveExercises":
                    CHResult = commandHandler.Expectation(HandlerAction.Save)
                                             .SaveExercisesCommand();
                    break;

                case "SettingExisting":
                    CHResult = commandHandler.SettingExistingCommand();
                    break;

                case "Selected":
                    CHResult = commandHandler.SelectedCommand();
                    break;

                case "ChangeActive":
                    CHResult = commandHandler.Expectation(HandlerAction.Save)
                                             .ChangeActiveCommand();
                    break;

                case "Archiving":
                    CHResult = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                             .ArchivingCommand();
                    break;

                case "Replace":
                    CHResult = commandHandler.ReplaceCommand();
                    break;

                case "ReplaceTo":
                    CHResult = commandHandler.Expectation(HandlerAction.Save)
                                             .ReplaceToCommand();
                    break;

                case "ChangeName":
                    CHResult = commandHandler.ChangeNameCommand();
                    break;

                case "ChangeMode":
                    CHResult = commandHandler.ChangeModeCommand();
                    break;

                case "ChangedMode":
                    CHResult = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                             .ChangedModeCommand();
                    break;

                case "Period":
                    CHResult = commandHandler.Period();
                    break;

                case "Delete":
                    CHResult = commandHandler.DeleteCommand();
                    break;

                case "ConfirmDelete":
                    CHResult = commandHandler.Expectation(HandlerAction.Remove, HandlerAction.Save)
                                             .ConfirmDeleteCommand();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            IInformationSet informationSet = CHResult.GetData();

            return informationSet;
        }

        private IInformationSet ProcessAdminCallBack(CommandHandlerData commandHandlerData, CallbackQueryParser callbackQueryParser)
        {
            AdminCH commandHandler = new AdminCH(commandHandlerData, callbackQueryParser);

            switch (callbackQueryParser.SubDirection)
            {
                case "Admin":
                    commandHandler.AdminCommand();
                    break;

                case "Logs":
                    commandHandler.LogsCommand();
                    break;

                case "ShowLastLog":
                    commandHandler.ShowLastLogCommand();
                    break;

                case "ShowLastExceptionLogs":
                    commandHandler.ShowLastExceptionLogsCommand();
                    break;

                case "FindLogByID":
                    commandHandler.FindLogByIDCommand(isEventID: false);
                    break;

                case "FindLogByEventID":
                    commandHandler.FindLogByIDCommand(isEventID: true);
                    break;

                case "ShowStartConfiguration":
                    commandHandler.ShowStartConfigurationCommand();
                    break;

                case "ChangeLimitsMods":
                    commandHandler.ChangeLimitsModsCommand();
                    break;
                
                case "ChangeWhiteListMode":
                    commandHandler.ChangeWhiteListModeCommand();
                    break;

                case "ChangeUserState":
                    commandHandler.ChangeUserStateCommand();
                    break;

                case "RemoveUser":
                    commandHandler.RemoveUserCommand();
                    break;

                case "DisableBot":
                    commandHandler.DisableBotCommand();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            IInformationSet informationSet = commandHandler.GetData();

            return informationSet;
        }

        private bool CheckingComplianceCallBackId(string currentCallBackId, out IInformationSet? informationSet)
        {
            if (CurrentUserContext.CallBackId == currentCallBackId)
            {
                informationSet = null;

                return true;
            }

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (CurrentUserContext.ActiveCycle == null)
            {
                responseConverter = new ResponseTextConverter("Начнём");
                buttonsSets = (ButtonsSet.AddCycle, ButtonsSet.None);
            }
            else
            {
                responseConverter = new ResponseTextConverter("Действие не может быть выполнено, т.к. информация устарела",
                        "Для продолжения работы используйте действия, предложенные ниже");
                buttonsSets = (ButtonsSet.Main, ButtonsSet.None);
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            CurrentUserContext.DataManager.ResetAll();

            return false;
        }
    }
}