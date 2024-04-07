#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.StepStore;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler.Context
{
    internal class CommonCH : CallBackCH
    {
        internal CommonCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) 
            : base(db, userContext, callbackQueryParser)
        { }

        internal override CommonCH Expectation(params HandlerAction[] handlerActions)
        {
            this.handlerActions = handlerActions;

            return this;
        }

        internal CommonCH BackCommand()
        {
            var previousStep = StepStorage.GetStep(callbackQueryParser.Args[2]);

            currentUserContext.Navigation.QueryFrom = previousStep.QueryFrom;
            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseConverter responseConverter = new ResponseConverter(previousStep.ButtonsSet switch // optional additional information
            {
                ButtonsSet.SettingCycle
                    => previousStep.Message + " " + currentUserContext.DataManager.CurrentCycle.Name,
                ButtonsSet.SettingDay
                    => previousStep.Message + " " + currentUserContext.DataManager.CurrentDay.Name,
                ButtonsSet.SettingExercise
                    => previousStep.Message + " " + currentUserContext.DataManager.CurrentExercise.Name,
                _ => previousStep.Message
            });

            (ButtonsSet, ButtonsSet) buttonsSets = (previousStep.ButtonsSet, previousStep.BackButtonsSet);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal CommonCH ToMainCommand()
        {
            currentUserContext.DataManager.ResetAll();

            var mainStep = StepStorage.GetMainStep();

            currentUserContext.Navigation.QueryFrom = mainStep.QueryFrom;
            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseConverter responseConverter = new ResponseConverter(mainStep.Message);
            (ButtonsSet, ButtonsSet) buttonsSets = (mainStep.ButtonsSet, mainStep.BackButtonsSet);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }
    }
}