#region using
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Buttons;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.MessageCommandHandler;
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

            switch (update.Type)
            {
                case UpdateType.Message:
                    await ProcessMessage(update);
                    break;
                case UpdateType.CallbackQuery:
                    await ProcessCallbackQuery(update);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный update.type: {update.Type}");
            }
        }

        private async Task ProcessMessage(Update update)
        {
            var requestConverter = new TextMessageConverter(update.Message.Text);

            var commandHandler = new TextMessageCH(db, CurrentUserContext, requestConverter);

            MessageInformationSet messageInformationSet;

            switch (CurrentUserContext.Navigation.MessageNavigationTarget)
            {
                case MessageNavigationTarget.Default:
                    messageInformationSet = commandHandler.Expectation()
                                                                    .DefaultCommand()
                                                                    .GetData();
                    break;

                case MessageNavigationTarget.AddCycle:
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Add, HandlerAction.Save)
                                                                                                        .AddCycleCommand()
                                                                                                        .GetData();
                    break;

                case MessageNavigationTarget.AddDays:
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Add, HandlerAction.Save)
                                                                                                        .AddDaysCommand()
                                                                                                        .GetData();
                    break;

                case MessageNavigationTarget.AddExercises:
                    messageInformationSet = commandHandler.Expectation()
                                                                    .AddExercisesCommand()
                                                                    .GetData();
                    break;

                case MessageNavigationTarget.AddResultForExercise:
                    messageInformationSet = commandHandler.Expectation()
                                                                    .AddResultForExerciseCommand()
                                                                    .GetData();
                    break;

                case MessageNavigationTarget.ChangeNameCycle:
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                            .ChangeNameCommand("Cycle")
                                                                                                            .GetData();
                    break;

                case MessageNavigationTarget.ChangeNameDay:
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                            .ChangeNameCommand("Day")
                                                                                                            .GetData();
                    break;

                case MessageNavigationTarget.ChangeNameExercise:
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                            .ChangeNameCommand("Exercise")
                                                                                                            .GetData();
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.MessageNavigationTarget: {CurrentUserContext.Navigation.MessageNavigationTarget}!");
            }

            await SendResponse(update.Message.Chat.Id, messageInformationSet.Message, messageInformationSet.ButtonsSets);
        }

        private async Task ProcessCallbackQuery(Update update)
        {
            var callbackQueryParser = new CallbackQueryParser(update.CallbackQuery.Data);

            if (!await CheckingComplianceCallBackId(callbackQueryParser.CallBackId))
                return;

            switch((CallBackNavigationTarget)callbackQueryParser.Direction)
            {
                case CallBackNavigationTarget.None:
                    await ProcessCommonCallBack(update, callbackQueryParser);
                    break;
                case CallBackNavigationTarget.Workout:
                    await ProcessWorkoutCallBack(update, callbackQueryParser);
                    break;
                case CallBackNavigationTarget.Analytics:
                    ProcessAnalyticsCallBack(update, callbackQueryParser);
                    break;
                case CallBackNavigationTarget.Settings:
                    await ProcessSettingsCallBack(update, callbackQueryParser);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallBackNavigationTarget: {(CallBackNavigationTarget)callbackQueryParser.Direction}");
            }
        }

        private async Task<bool> CheckingComplianceCallBackId(string currentCallBackId)
        {
            if (CurrentUserContext.CallBackId != currentCallBackId)
            {
                var responseConverter = new ResponseConverter("Действие не может быть выполнено, т.к. информация устарела",
                        "Для продолжения работы используйте последний диалог или введите команду /Start");
                var buttonsSet = (ButtonsSet.None, ButtonsSet.None);

                await SendResponse(CurrentUserContext.UserInformation.UserId, responseConverter.Convert(), buttonsSet);

                return false;
            }

            return true;
        }

        private async Task ProcessCommonCallBack(Update update, CallbackQueryParser callbackQueryParser)
        {
            var commandHandler = new CommonCH(db, CurrentUserContext, callbackQueryParser);

            MessageInformationSet messageInformationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Back":
                    messageInformationSet = commandHandler
                                                        .BackCommand()
                                                        .GetData();
                    break;

                case "ToMain":
                    messageInformationSet = commandHandler
                                                        .ToMainCommand()
                                                        .GetData();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            await SendResponse(update.CallbackQuery.Message.Chat.Id, messageInformationSet.Message, messageInformationSet.ButtonsSets);
        }

        private async Task ProcessWorkoutCallBack(Update update, CallbackQueryParser callbackQueryParser)
        {
            var commandHandler = new WorkoutCH(db, CurrentUserContext, callbackQueryParser);

            MessageInformationSet messageInformationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Workout":
                    messageInformationSet = commandHandler
                                                        .WorkoutCommand()
                                                        .GetData();
                    break;

                case "LastResult":
                    messageInformationSet = commandHandler
                                                        .LastResultCommand()
                                                        .GetData();
                    break;

                case "SaveResultsExercise":
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                        .SaveResultsExerciseCommand()
                                                                                        .GetData();
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            await SendResponse(update.CallbackQuery.Message.Chat.Id, messageInformationSet.Message, messageInformationSet.ButtonsSets);
        }

        private void ProcessAnalyticsCallBack(Update update, CallbackQueryParser callbackQueryParser)
        {
            ResponseConverter responseConverter;

            switch (callbackQueryParser.SubDirection)
            {
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }
        }

        private async Task ProcessSettingsCallBack(Update update, CallbackQueryParser callbackQueryParser)
        {
            var commandHandler = new SettingsCH(db, CurrentUserContext, callbackQueryParser);

            MessageInformationSet messageInformationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Settings":
                    messageInformationSet = commandHandler
                                                        .SettingsCommand()
                                                        .GetData();
                    break;

                case "ArchiveStore":
                    messageInformationSet = commandHandler
                                                        .ArchiveStoreCommand()
                                                        .GetData();
                    break;

                case "Archive":
                    messageInformationSet = commandHandler
                                                        .ArchiveCommand()
                                                        .GetData();
                    break;

                case "UnArchive":
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                            .UnArchiveCommand()
                                                                                                            .GetData();
                    break;

                case "Setting":
                    messageInformationSet = commandHandler
                                                        .SettingCommand()
                                                        .GetData();
                    break;

                case "Add":
                    messageInformationSet = commandHandler
                                                        .AddCommand()
                                                        .GetData();
                    break;

                case "SaveExercises":
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                        .SaveExercisesCommand()
                                                                                        .GetData();
                    break;

                case "SettingExisting":
                    messageInformationSet = commandHandler
                                                        .SettingExistingCommand()
                                                        .GetData();
                    break;

                case "Selected":
                    messageInformationSet = commandHandler
                                                        .SelectedCommand()
                                                        .GetData();
                    break;

                case "ChangeActive":
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                        .ChangeActiveCommand()
                                                                                        .GetData();
                    break;

                case "Archiving":
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Update, HandlerAction.Save)
                                                                                                            .ArchivingCommand()
                                                                                                            .GetData();
                    break;

                case "Replace":
                    messageInformationSet = commandHandler
                                                        .ReplaceCommand()
                                                        .GetData();
                    break;

                case "ReplaceTo":
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Save)
                                                                                        .ReplaceCommand()
                                                                                        .GetData();
                    break;

                case "ChangeName":
                    messageInformationSet = commandHandler
                                                        .ChangeNameCommand()
                                                        .GetData();
                    break;

                case "Delete":
                    messageInformationSet = commandHandler
                                                        .DeleteCommand()
                                                        .GetData();
                    break;

                case "ConfirmDelete":
                    messageInformationSet = commandHandler.Expectation(HandlerAction.Remove, HandlerAction.Save)
                                                                                                            .ConfirmDeleteCommand()
                                                                                                            .GetData();
                    break;

                case "ConfirmDeleteAccount":
                    DeleteAccount();

                    messageInformationSet = new MessageInformationSet("Аккаунт успешно удалён", (ButtonsSet.None, ButtonsSet.None));
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            await SendResponse(update.CallbackQuery.Message.Chat.Id, messageInformationSet.Message, messageInformationSet.ButtonsSets);
        }

        private void DeleteAccount()
        {
            primaryProcessUpdate.DeleteContext(CurrentUserContext.UserInformation.UserId);
            adminHandler.DeleteAccount(CurrentUserContext.UserInformation);
        }

        private async Task SendResponse(long chatId, string message, (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) ButtonsSets)
        {
            var buttons = new InlineButtons(CurrentUserContext);

            await botClient.SendTextMessageAsync(chatId,
                                            message,
                                            replyMarkup: buttons.GetInlineButtons(ButtonsSets.buttonsSet, ButtonsSets.backButtonsSet));
        }
    }
}