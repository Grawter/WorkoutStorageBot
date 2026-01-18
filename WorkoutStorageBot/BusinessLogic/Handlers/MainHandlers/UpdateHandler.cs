using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

namespace WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers
{
    internal class UpdateHandler : CoreHandler
    {
        internal UpdateHandler(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(PrimaryUpdateHandler))
        { }

        internal override async Task<HandlerResult> Process(HandlerResult handlerResult)
        {
            UpdateHandlerResult updateHandlerResult = CreateUpdateHandlerResult((AuthorizedHandlerResult)handlerResult);

            switch (updateHandlerResult.ShortUpdateInfo.UpdateType)
            {
                case UpdateType.Message:
                    updateHandlerResult.InformationSet = await ProcessMessage(updateHandlerResult);
                    break;
                case UpdateType.CallbackQuery:
                    updateHandlerResult.InformationSet = await ProcessCallbackQuery(updateHandlerResult);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный update.type: {updateHandlerResult.ShortUpdateInfo.UpdateType}");
            }

            return updateHandlerResult;
        }

        private UpdateHandlerResult CreateUpdateHandlerResult(AuthorizedHandlerResult authorizedHandlerResult)
        {
            return new UpdateHandlerResult()
            {
                Update = authorizedHandlerResult.Update,
                CurrentUserContext = authorizedHandlerResult.CurrentUserContext,
                ShortUpdateInfo = authorizedHandlerResult.ShortUpdateInfo,
                HasAccess = true,
            };
        }

        private async Task<IInformationSet> ProcessMessage(UpdateHandlerResult updateHandlerResult)
        {
            CommandHandlerData commandHandlerData = new CommandHandlerData()
            {
                Db = CoreTools.Db,
                ParentHandler = this,
                CurrentUserContext = updateHandlerResult.CurrentUserContext,
            };

            TextMessageConverter requestConverter = new TextMessageConverter(updateHandlerResult.ShortUpdateInfo.Data);

            TextMessageCH commandHandler = new TextMessageCH(commandHandlerData, requestConverter);

            return await commandHandler.GetInformationSet();
        }

        private async Task<IInformationSet> ProcessCallbackQuery(UpdateHandlerResult updateHandlerResult)
        {
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser(updateHandlerResult.ShortUpdateInfo.Data);

            if (CheckingComplianceCallBackId(updateHandlerResult.CurrentUserContext, callbackQueryParser.CallBackId, out IInformationSet? informationSet))
            {
                CommandHandlerData commandHandlerData = new CommandHandlerData()
                {
                    Db = CoreTools.Db,
                    ParentHandler = this,
                    CurrentUserContext = updateHandlerResult.CurrentUserContext,
                };

                CallBackCH commandHandler = GetCallBackCH(commandHandlerData, callbackQueryParser);

                informationSet = await commandHandler.GetInformationSet();
            }

            return informationSet;
        }

        private CallBackCH GetCallBackCH(CommandHandlerData commandHandlerData, CallbackQueryParser callbackQueryParser)
        {
            CallBackCH? commandHandler;

            switch ((CallBackNavigationTarget)callbackQueryParser.Direction)
            {
                case CallBackNavigationTarget.None:
                    commandHandler = new CommonCH(commandHandlerData, callbackQueryParser);
                    break;
                case CallBackNavigationTarget.Workout:
                    commandHandler = new WorkoutCH(commandHandlerData, callbackQueryParser);
                    break;
                case CallBackNavigationTarget.Settings:
                    commandHandler = new SettingsCH(commandHandlerData, callbackQueryParser);
                    break;
                case CallBackNavigationTarget.Admin:
                    commandHandler = new AdminCH(commandHandlerData, callbackQueryParser);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallBackNavigationTarget: {(CallBackNavigationTarget)callbackQueryParser.Direction}");
            }

            return commandHandler;
        }

        private bool CheckingComplianceCallBackId(UserContext currentUserContext, string currentCallBackId, [NotNullWhen(false)] out IInformationSet? informationSet)
        {
            if (currentUserContext.CallBackId == currentCallBackId)
            {
                informationSet = null;

                return true;
            }

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (currentUserContext.ActiveCycle == null)
            {
                responseConverter = new ResponseTextConverter("Начнём");
                buttonsSets = (ButtonsSet.AddCycle, ButtonsSet.None);
            }
            else
            {
                responseConverter = new ResponseTextConverter("Действие не может быть выполнено, т.к. информация устарела",
                        "Для продолжения работы используйте действия, предложенные ниже");
                buttonsSets = (ButtonsSet.Main, ButtonsSet.None);

                currentUserContext.Navigation.ResetNavigation();
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            currentUserContext.DataManager.ResetAll();

            return false;
        }
    }
}