using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler.Context;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.Core.Handlers.Abstraction;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers
{
    internal class UpdateHandler : CoreHandler
    {
        protected override ILogger Logger { get; }

        internal UpdateHandler(CoreTools coreTools, RepositoriesStore repositoriesStore) : base(coreTools, repositoriesStore, nameof(UpdateHandler))
        {
            this.Logger = CoreTools.LoggerFactory.CreateLogger<UpdateHandler>();
        }

        internal override async Task<HandlerResult> Process(HandlerResult handlerResult, CancellationToken cancellationToken)
        {
            UpdateHandlerResult updateHandlerResult = CreateUpdateHandlerResult((AuthorizedHandlerResult)handlerResult);

            CommandHandlerTools commandHandlerData = new CommandHandlerTools()
            {
                CoreTools = this.CoreTools,
                HandlerResult = updateHandlerResult,
                RepositoriesStore = this.RepositoriesStore,
            };

            switch (updateHandlerResult.ShortUpdateInfo.UpdateType)
            {
                case UpdateType.Message:
                    await ProcessMessage(commandHandlerData, cancellationToken);
                    break;
                case UpdateType.CallbackQuery:
                    await ProcessCallbackQuery(commandHandlerData, cancellationToken);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный UpdateType: {updateHandlerResult.ShortUpdateInfo.UpdateType}");
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
            };
        }

        private async Task ProcessMessage(CommandHandlerTools commandHandlerTools, CancellationToken cancellationToken)
        {
            MessageTextBuilder requestTextBuilder = new MessageTextBuilder(commandHandlerTools.HandlerResult.ShortUpdateInfo.Data);

            TextMessageCH commandHandler = new TextMessageCH(commandHandlerTools, requestTextBuilder);

            await commandHandler.Process(cancellationToken);
        }

        private async Task ProcessCallbackQuery(CommandHandlerTools commandHandlerTools, CancellationToken cancellationToken)
        {
            CallbackQueryParser callbackQueryParser = new CallbackQueryParser(commandHandlerTools.HandlerResult.ShortUpdateInfo.Data);

            if (CheckingComplianceCallBackId(commandHandlerTools.HandlerResult.CurrentUserContext, callbackQueryParser))
            {
                CallBackCH commandHandler = GetCallBackCH(commandHandlerTools, callbackQueryParser);

                await commandHandler.Process(cancellationToken);
            }
            else
                SetupInformationSetForNonCheckingComplianceCallBackId(commandHandlerTools.HandlerResult);
        }

        private CallBackCH GetCallBackCH(CommandHandlerTools commandHandlerData, CallbackQueryParser callbackQueryParser)
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

        private bool CheckingComplianceCallBackId(UserContext userContext, CallbackQueryParser callbackQueryParser)
            => userContext.CallBackId == callbackQueryParser.CallBackId;

        private void SetupInformationSetForNonCheckingComplianceCallBackId(AuthorizedHandlerResult handlerResult)
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (handlerResult.CurrentUserContext.ActiveCycle == null)
            {
                responseTextBuilder = new ResponseTextBuilder("Начнём");
                buttonsSets = (ButtonsSet.AddCycle, ButtonsSet.None);
            }
            else
            {
                responseTextBuilder = new ResponseTextBuilder("Действие не может быть выполнено, т.к. информация устарела",
                        "Для продолжения работы используйте действия, предложенные ниже");
                buttonsSets = (ButtonsSet.Main, ButtonsSet.None);

                handlerResult.CurrentUserContext.Navigation.ResetNavigation();
            }

            handlerResult.IsNeedSendResponse = true;
            handlerResult.InformationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            handlerResult.CurrentUserContext.DataManager.ResetAll();
        }
    }
}