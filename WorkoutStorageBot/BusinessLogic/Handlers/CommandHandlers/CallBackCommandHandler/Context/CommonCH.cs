#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.StepStore;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.HandlerData;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class CommonCH : CallBackCH
    {
        internal CommonCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override CommonCH Expectation(params HandlerAction[] handlerActions)
        {
            this.HandlerActions = handlerActions;

            return this;
        }

        internal CommonCH BackCommand()
        {
            string buttonsSet = callbackQueryParser.GetRequiredParameter(2);

            StepInformation previousStep = StepStorage.GetStep(buttonsSet);

            this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom = previousStep.QueryFrom;
            this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseTextConverter responseConverter = new ResponseTextConverter(previousStep.ButtonsSet switch // optional additional information
            {
                ButtonsSet.SettingCycle
                    => $"{previousStep.Message} {CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}",
                ButtonsSet.SettingDay
                    => $"{previousStep.Message} {CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}",
                ButtonsSet.SettingExercise
                    => $"{previousStep.Message} {CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}",
                _ => previousStep.Message
            });

            (ButtonsSet, ButtonsSet) buttonsSets = (previousStep.ButtonsSet, previousStep.BackButtonsSet);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal CommonCH ToMainCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetAll();

            StepInformation mainStep = StepStorage.GetMainStep();

            this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom = mainStep.QueryFrom;
            this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseTextConverter responseConverter = new ResponseTextConverter(mainStep.Message);
            (ButtonsSet, ButtonsSet) buttonsSets = (mainStep.ButtonsSet, mainStep.BackButtonsSet);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }
    }
}