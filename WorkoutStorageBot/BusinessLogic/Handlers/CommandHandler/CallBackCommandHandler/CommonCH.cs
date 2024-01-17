#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.StepStore;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler
{
    internal class CommonCH : CallBackCH
    {
        internal CommonCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) : base(db, userContext, callbackQueryParser)
        { }

        internal override CommonCH Expectation(params HandlerAction[] handlerActions)
        {
            this.handlerActions = handlerActions;

            return this;
        }

        internal override MessageInformationSet GetData()
        {
            foreach (var handlerAction in handlerActions)
            {
                switch (handlerAction)
                {
                    case HandlerAction.None:
                        break;
                    case HandlerAction.Update:
                        db.Update(domain);
                        break;
                    case HandlerAction.Add:
                        db.Add(domain);
                        break;
                    case HandlerAction.Remove:
                        db.Remove(domain);
                        break;
                    case HandlerAction.Save:
                        db.SaveChanges();
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный handlerAction: {handlerAction}");
                }

            }

            return new MessageInformationSet(responseConverter.Convert(), buttonsSets);
        }

        internal CommonCH BackCommand()
        {
            var previousStep = StepStorage.GetStep(callbackQueryParser.Args[2]);

            currentUserContext.Navigation.QueryFrom = previousStep.QueryFrom;
            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            responseConverter = new ResponseConverter(previousStep.ButtonsSet switch // optional additional information
            {
                ButtonsSet.SettingCycle
                    => previousStep.Message + " " + currentUserContext.DataManager.CurrentCycle.Name,
                ButtonsSet.SettingDay
                    => previousStep.Message + " " + currentUserContext.DataManager.CurrentDay.Name,
                ButtonsSet.SettingExercise
                    => previousStep.Message + " " + currentUserContext.DataManager.CurrentExercise.Name,
                _ => previousStep.Message
            });

            buttonsSets = (previousStep.ButtonsSet, previousStep.BackButtonsSet);

            return this;
        }

        internal CommonCH ToMainCommand()
        {
            currentUserContext.DataManager.ResetAll();

            var mainStep = StepStorage.GetMainStep();

            currentUserContext.Navigation.QueryFrom = mainStep.QueryFrom;
            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            responseConverter = new ResponseConverter(mainStep.Message);
            buttonsSets = (mainStep.ButtonsSet, mainStep.BackButtonsSet);

            return this;
        }
    }
}