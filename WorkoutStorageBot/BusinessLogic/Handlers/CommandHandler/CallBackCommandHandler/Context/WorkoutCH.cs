#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
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
            string information;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.ObjectType)
            {
                case "Exercises":
                    IEnumerable<int> dayIDs = currentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive).Select(d => d.Id);

                    IQueryable<Exercise> exercises = db.Exercises.Where(e => !e.IsArchive && dayIDs.Contains(e.DayId));

                    IGrouping<DateTime, ResultExercise> resultLastTraining = db.ResultsExercises
                                                                .Where(re => exercises.Select(e => e.Id).Contains(re.ExerciseId))
                                                                .OrderByDescending(re => re.DateTime)
                                                                .GroupBy(re => re.DateTime)
                                                                .AsEnumerable()
                                                                .LastOrDefault();

                    information = ResponseConverter.GetInformationAboutLastExercises(exercises, resultLastTraining);
                    responseConverter = new ResponseConverter("Последняя тренировка:", information, "Выберите тренировочный день");
                    buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;
                case "Day":
                    IEnumerable<int> exercisesIDs = currentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive)
                                                                                   .Select(d => d.Id);

                    IQueryable<ResultExercise> lastDateForExercises = db.ResultsExercises.Where(re => exercisesIDs.Contains(re.ExerciseId))
                                                .OrderBy(re => re.ExerciseId)
                                                .ThenByDescending(re => re.DateTime)
                                                .GroupBy(re => re.ExerciseId)
                                                //order Data in group and get first (older) element for get lastDate
                                                .Select(reGROUP => reGROUP.OrderByDescending(re => re.DateTime).First());

                    IQueryable<ResultExercise> lastResultsExercisesInCurrentDay = default;
                    bool isFirstQuery = true;
                    foreach (ResultExercise resultExercise in lastDateForExercises)
                    {
                        if (isFirstQuery)
                        {
                            lastResultsExercisesInCurrentDay = db.ResultsExercises.Where(re => re.ExerciseId == resultExercise.ExerciseId && 
                                                                                         re.DateTime.Date == resultExercise.DateTime.Date);
                            isFirstQuery = false;
                        }
                        else
                            lastResultsExercisesInCurrentDay = lastResultsExercisesInCurrentDay.Union(db.ResultsExercises.Where(re => re.ExerciseId == resultExercise.ExerciseId && 
                                                                                                                                re.DateTime.Date == resultExercise.DateTime.Date));
                    }

                    information = ResponseConverter.GetInformationAboutLastDay(currentUserContext.DataManager.CurrentDay.Exercises, lastResultsExercisesInCurrentDay);
                    responseConverter = new ResponseConverter("Последняя результаты упражнений из этого дня:", information, "Выберите упражнение");
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