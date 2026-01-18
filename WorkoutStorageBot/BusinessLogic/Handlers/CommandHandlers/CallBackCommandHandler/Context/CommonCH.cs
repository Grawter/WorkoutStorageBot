using WorkoutStorageBot.BusinessLogic.Context.StepStore;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.DTO.HandlerData;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class CommonCH : CallBackCH
    {
        internal CommonCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override Task<IInformationSet> GetInformationSet()
        {
            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Back":
                    informationSet = BackCommand();
                    break;

                case "ToMain":
                    informationSet = ToMainCommand();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            CheckInformationSet(informationSet);

            return Task.FromResult<IInformationSet>(informationSet);
        }

        private IInformationSet BackCommand()
        {
            string buttonsSet = callbackQueryParser.AdditionalParameters.First();

            StepInformation previousStep = StepStorage.GetStep(buttonsSet);

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetQueryFrom(previousStep.QueryFrom);
            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextConverter responseConverter = new ResponseTextConverter(previousStep.ButtonsSet switch // optional additional information
            {
                ButtonsSet.SettingCycle
                    => $"{previousStep.Message} {CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}",
                ButtonsSet.SettingDay
                    => $"{previousStep.Message} {CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}",
                ButtonsSet.SettingExercise
                    => $"{previousStep.Message} {CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}",
                _ => previousStep.Message
            });

            (ButtonsSet, ButtonsSet) buttonsSets = (previousStep.ButtonsSet, previousStep.BackButtonsSet);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ToMainCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetAll();

            StepInformation mainStep = StepStorage.GetMainStep();

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetQueryFrom(mainStep.QueryFrom);
            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextConverter responseConverter = new ResponseTextConverter(mainStep.Message);
            (ButtonsSet, ButtonsSet) buttonsSets = (mainStep.ButtonsSet, mainStep.BackButtonsSet);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }
    }
}