#region using
using OfficeOpenXml;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Buttons;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.MessageCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using WorkoutStorageBot.Helpers.Logger;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers
{
    internal class TelegramBotHandler
    {
        private readonly ITelegramBotClient botClient;
        private readonly EntityContext db;
        private readonly PrimaryUpdateHandler primaryProcessUpdate;
        private readonly AdminHandler adminHandler;
        private UserContext? currentUserContext;
        private UserContext? CurrentUserContext { get => currentUserContext; set => currentUserContext = value; }
        internal bool WhiteList { get => primaryProcessUpdate.WhiteList; set => primaryProcessUpdate.WhiteList = value; }

        internal TelegramBotHandler(ITelegramBotClient botClient, EntityContext db, ILogger logger, AdminHandler adminHandler)
        {
            this.botClient = botClient;
            this.db = db;

            primaryProcessUpdate = new PrimaryUpdateHandler(logger, botClient, db);

            this.adminHandler = adminHandler;
        }

        internal async Task ProcessUpdate(Update update)
        {
            primaryProcessUpdate.Execute(update, out currentUserContext);
            if (!primaryProcessUpdate.HasAccess || primaryProcessUpdate.IsNewContext)
                return;

            IInformationSet informationSet;

            switch (update.Type)
            {
                case UpdateType.Message:
                    informationSet = ProcessMessage(update);
                    break;
                case UpdateType.CallbackQuery:
                    informationSet = ProcessCallbackQuery(update);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный update.type: {update.Type}");
            }

            await SendResponse(update.Message.Chat.Id, informationSet);
        }

        private IInformationSet ProcessMessage(Update update)
        {
            var requestConverter = new TextMessageConverter(update.Message.Text);

            var commandHandler = new TextMessageCH(db, CurrentUserContext, requestConverter);

            IInformationSet informationSet;

            switch (CurrentUserContext.Navigation.MessageNavigationTarget)
            {
                case MessageNavigationTarget.Default:
                    informationSet = commandHandler.Expectation()
                                                                .DefaultCommand()
                                                                .GetData();
                    break;

                case MessageNavigationTarget.AddCycle:
                    informationSet = commandHandler.Expectation(HandlerAction.Add, HandlerAction.Save)
                                                                                                    .AddCycleCommand()
                                                                                                    .GetData();
                    break;

                case MessageNavigationTarget.AddDays:
                    informationSet = commandHandler.Expectation(HandlerAction.Add, HandlerAction.Save)
                                                                                                    .AddDaysCommand()
                                                                                                    .GetData();
                    break;

                case MessageNavigationTarget.AddExercises:
                    informationSet = commandHandler.Expectation()
                                                                .AddExercisesCommand()
                                                                .GetData();
                    break;

                case MessageNavigationTarget.AddResultForExercise:
                    informationSet = commandHandler.Expectation()
                                                                .AddResultForExerciseCommand()
                                                                .GetData();
                    break;

                case MessageNavigationTarget.ChangeNameCycle:
                    informationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                        .ChangeNameCommand("Cycle")
                                                                                                        .GetData();
                    break;

                case MessageNavigationTarget.ChangeNameDay:
                    informationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                        .ChangeNameCommand("Day")
                                                                                                        .GetData();
                    break;

                case MessageNavigationTarget.ChangeNameExercise:
                    informationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                        .ChangeNameCommand("Exercise")
                                                                                                        .GetData();
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.MessageNavigationTarget: {CurrentUserContext.Navigation.MessageNavigationTarget}!");
            }

            return informationSet;
        }

        private IInformationSet ProcessCallbackQuery(Update update)
        {
            var callbackQueryParser = new CallbackQueryParser(update.CallbackQuery.Data);

            if (CheckingComplianceCallBackId(callbackQueryParser.CallBackId, out IInformationSet informationSet))
            {
                switch ((CallBackNavigationTarget)callbackQueryParser.Direction)
                {
                    case CallBackNavigationTarget.None:
                        informationSet = ProcessCommonCallBack(update, callbackQueryParser);
                        break;
                    case CallBackNavigationTarget.Workout:
                        informationSet = ProcessWorkoutCallBack(update, callbackQueryParser);
                        break;
                    case CallBackNavigationTarget.Settings:
                        informationSet = ProcessSettingsCallBack(update, callbackQueryParser);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный CallBackNavigationTarget: {(CallBackNavigationTarget)callbackQueryParser.Direction}");
                }
            }

            return informationSet;
        }

        private IInformationSet ProcessCommonCallBack(Update update, CallbackQueryParser callbackQueryParser)
        {
            var commandHandler = new CommonCH(db, CurrentUserContext, callbackQueryParser);

            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Back":
                    informationSet = commandHandler
                                                .BackCommand()
                                                .GetData();
                    break;

                case "ToMain":
                    informationSet = commandHandler
                                                .ToMainCommand()
                                                .GetData();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            return informationSet;
        }

        private IInformationSet ProcessWorkoutCallBack(Update update, CallbackQueryParser callbackQueryParser)
        {
            var commandHandler = new WorkoutCH(db, CurrentUserContext, callbackQueryParser);

            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Workout":
                    informationSet = commandHandler
                                                .WorkoutCommand()
                                                .GetData();
                    break;

                case "LastResult":
                    informationSet = commandHandler
                                                .LastResultCommand()
                                                .GetData();
            break;

                case "SaveResultsExercise":
                    informationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                .SaveResultsExerciseCommand()
                                                                                .GetData();
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            return informationSet;
        }

        private IInformationSet ProcessSettingsCallBack(Update update, CallbackQueryParser callbackQueryParser)
        {
            var commandHandler = new SettingsCH(db, CurrentUserContext, callbackQueryParser);

            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Settings":
                    informationSet = commandHandler
                                                .SettingsCommand()
                                                .GetData();
                    break;

                case "ArchiveStore":
                    informationSet = commandHandler
                                                .ArchiveStoreCommand()
                                                .GetData();
                    break;

                case "Archive":
                    informationSet = commandHandler
                                                .ArchiveCommand()
                                                .GetData();

                    break;

                case "ExportTo":
                    informationSet = commandHandler
                                                .ExportToCommand()
                                                .GetData();

                    break;

                case "UnArchive":
                    informationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                        .UnArchiveCommand()
                                                                                                        .GetData();
                    break;

                case "Setting":
                    informationSet = commandHandler
                                                .SettingCommand()
                                                .GetData();
                    break;

                case "Add":
                    informationSet = commandHandler
                                                .AddCommand()
                                                .GetData();
                    break;

                case "SaveExercises":
                    informationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                .SaveExercisesCommand()
                                                                                .GetData();
                    break;

                case "SettingExisting":
                    informationSet = commandHandler
                                                .SettingExistingCommand()
                                                .GetData();
                    break;

                case "Selected":
                    informationSet = commandHandler
                                                .SelectedCommand()
                                                .GetData();
                    break;

                case "ChangeActive":
                    informationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                .ChangeActiveCommand()
                                                                                .GetData();
                    break;

                case "Archiving":
                    informationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                        .ArchivingCommand()
                                                                                                        .GetData();
                    break;

                case "Replace":
                    informationSet = commandHandler
                                                        .ReplaceCommand()
                                                        .GetData();
                    break;

                case "ReplaceTo":
                    informationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                .ReplaceCommand()
                                                                                .GetData();
                    break;

                case "ChangeName":
                    informationSet = commandHandler
                                                .ChangeNameCommand()
                                                .GetData();
                    break;

                case "Period":
                    informationSet = commandHandler
                                                .Period()
                                                .GetData();

                    break;

                case "Delete":
                    informationSet = commandHandler
                                                .DeleteCommand()
                                                .GetData();
                    break;

                case "ConfirmDelete":
                    informationSet = commandHandler.Expectation(HandlerAction.Remove, HandlerAction.Save)
                                                                                                        .ConfirmDeleteCommand()
                                                                                                        .GetData();
                    break;

                case "ConfirmDeleteAccount":
                    DeleteAccount();

                    informationSet = new MessageInformationSet("Аккаунт успешно удалён", (ButtonsSet.None, ButtonsSet.None));
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            return informationSet;
        }

        private void DeleteAccount()
        {
            primaryProcessUpdate.DeleteContext(CurrentUserContext.UserInformation.UserId);
            adminHandler.DeleteAccount(CurrentUserContext.UserInformation);
        }

        private async Task SendResponse(long chatId, IInformationSet messageInformationSetting)
        {
            var buttons = new InlineButtons(CurrentUserContext);

            switch (messageInformationSetting)
            {
                case MessageInformationSet MISet:
                    await botClient.SendTextMessageAsync(chatId,
                                            messageInformationSetting.Message,
                                            replyMarkup: buttons.GetInlineButtons(messageInformationSetting.ButtonsSets, messageInformationSetting.AdditionalParameters));
                    break;

                case FileInformationSet FISet:
                    await botClient.SendDocumentAsync(chatId,
                                                    document: InputFile.FromStream(stream: FISet.Stream, fileName: FISet.FileName),
                                                    caption: FISet.Message,
                                                    replyMarkup: buttons.GetInlineButtons(messageInformationSetting.ButtonsSets, messageInformationSetting.AdditionalParameters));

                    FISet.Stream.Dispose();
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный messageInformationSetting: {messageInformationSetting.GetType()}");
            }
        }

        private bool CheckingComplianceCallBackId(string currentCallBackId, out IInformationSet informationSet)
        {
            if (CurrentUserContext.CallBackId != currentCallBackId)
            {
                var responseConverter = new ResponseConverter("Действие не может быть выполнено, т.к. информация устарела",
                        "Для продолжения работы используйте последний диалог или введите команду /Start");
                var buttonsSet = (ButtonsSet.None, ButtonsSet.None);

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSet);

                return false;
            }

            informationSet = null;

            return true;
        }
    }
}