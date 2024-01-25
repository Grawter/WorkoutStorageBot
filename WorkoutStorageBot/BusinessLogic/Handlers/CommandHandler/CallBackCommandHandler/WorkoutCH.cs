#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.SQLiteQueries;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler
{
    internal class WorkoutCH : CallBackCH
    {
        internal WorkoutCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) : base(db, userContext, callbackQueryParser)
        { }

        internal override WorkoutCH Expectation(params HandlerAction[] handlerActions)
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

        internal WorkoutCH WorkoutCommand()
        {
            currentUserContext.Navigation.QueryFrom = QueryFrom.NoMatter; // not necessary, but just in case

            responseConverter = new ResponseConverter("Выберите тренировочный день");
            buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

            return this;
        }

        internal WorkoutCH LastResultCommand()
        {
            IEnumerable<ResultExercise> trainingIndicators;
            string information;

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

            return this;
        }

        internal WorkoutCH SaveResultsExerciseCommand()
        {
            db.ResultsExercises.AddRange(currentUserContext.DataManager.ResultExercises);

            currentUserContext.DataManager.ResetResultExercises();

            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            responseConverter = new ResponseConverter("Введённые данные сохранены!", "Выберите упраженение");
            buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            return this;
        }
    }
}