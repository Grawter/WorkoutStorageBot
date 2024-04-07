#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.SQLiteQueries;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler.Context
{
    internal class WorkoutCH : CallBackCH
    {
        internal WorkoutCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) 
            : base(db, userContext, callbackQueryParser)
        { }

        internal override WorkoutCH Expectation(params HandlerAction[] handlerActions)
        {
            this.handlerActions = handlerActions;

            return this;
        }

        internal WorkoutCH WorkoutCommand()
        {
            currentUserContext.Navigation.QueryFrom = QueryFrom.NoMatter; // not necessary, but just in case
            
            ResponseConverter responseConverter = new ResponseConverter("Выберите тренировочный день");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal WorkoutCH LastResultCommand()
        {
            ResponseConverter responseConverter;
            IEnumerable<ResultExercise> trainingIndicators;
            string information;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.ObjectType)
            {
                case "Exercises":
                    var exercises = QueriesStorage.GetExercisesWithDaysIds(currentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive).Select(d => d.Id), db.GetExercisesFromQuery);
                    trainingIndicators = QueriesStorage.GetLastResultsExercisesWithExercisesIds(exercises.Select(e => e.Id), db.GetResultExercisesFromQuery);

                    information = ResponseConverter.GetInformationAboutLastExercises(exercises, trainingIndicators);
                    responseConverter = new ResponseConverter("Последняя тренировка:", information, "Выберите тренировочный день");
                    buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;
                case "Day":
                    var lastDateForExercises = QueriesStorage.GetLastDateForExercises(currentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive).Select(d => d.Id), db.GetResultExercisesFromQuery);
                    trainingIndicators = QueriesStorage.GetLastResultsForExercisesAndDate(lastDateForExercises, db.GetResultExercisesFromQuery);

                    information = ResponseConverter.GetInformationAboutLastDay(currentUserContext.DataManager.CurrentDay.Exercises, trainingIndicators);
                    responseConverter = new ResponseConverter("Последняя результаты упражений из этого дня:", information, "Выберите упражнение");
                    buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal WorkoutCH SaveResultsExerciseCommand()
        {
            db.ResultsExercises.AddRange(currentUserContext.DataManager.ResultExercises);

            currentUserContext.DataManager.ResetResultExercises();

            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseConverter responseConverter = new ResponseConverter("Введённые данные сохранены!", "Выберите упраженение");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }
    }
}