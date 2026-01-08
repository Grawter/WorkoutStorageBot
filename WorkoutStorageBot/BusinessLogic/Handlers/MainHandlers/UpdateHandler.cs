using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

namespace WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers
{
    internal class UpdateHandler : CoreHandler
    {
        private UserContext CurrentUserContext { get; set; }

        internal UpdateHandler(CoreTools coreTools, CoreManager coreManager) : base(coreTools, coreManager, nameof(PrimaryUpdateHandler))
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

            return commandHandler.GetInformationSet();
        }

        private IInformationSet ProcessCallbackQuery(ShortUpdateInfo updateInfo)
        {
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser(updateInfo.Data);

            if (CheckingComplianceCallBackId(callbackQueryParser.CallBackId, out IInformationSet? informationSet))
            {
                CommandHandlerData commandHandlerData = new CommandHandlerData()
                {
                    Db = CoreTools.Db,
                    ParentHandler = this,
                    CurrentUserContext = CurrentUserContext,
                };

                CallBackCH commandHandler = GetCallBackCH(commandHandlerData, callbackQueryParser);

                informationSet = commandHandler.GetInformationSet();
            }

            // Не обязательно. Чтобы не было анимации "зависание кнопки" в ТГ боте
            informationSet.AdditionalParameters.Add("BotCallBackID", updateInfo.Update.CallbackQuery.Id);

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

        private bool CheckingComplianceCallBackId(string currentCallBackId, [NotNullWhen(false)] out IInformationSet? informationSet)
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

                CurrentUserContext.Navigation.ResetNavigation();
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            CurrentUserContext.DataManager.ResetAll();

            return false;
        }
    }
}