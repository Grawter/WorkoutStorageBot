#region using
using System.Text;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.Domain;
using WorkoutStorageBot.Model.HandlerData;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class WorkoutCH : CallBackCH
    {
        internal WorkoutCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override WorkoutCH Expectation(params HandlerAction[] handlerActions)
        {
            this.HandlerActions = handlerActions;

            return this;
        }

        internal WorkoutCH WorkoutCommand()
        {
            CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom = QueryFrom.NoMatter; // not necessary, but just in case
            
            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите тренировочный день");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal WorkoutCH LastResultCommand()
        {
            ResponseTextConverter responseConverter;
            string information;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Exercises":
                    IEnumerable<int> activeDayIDs = CommandHandlerTools.CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive)
                                                                                                           .Select(d => d.Id);

                    IQueryable<Exercise> activeExercisesInActiveDays = CommandHandlerTools.Db.Exercises.Where(e => !e.IsArchive && activeDayIDs.Contains(e.DayId));

                    IQueryable<int> activeExercisesIDsInActiveDays = activeExercisesInActiveDays.Select(e => e.Id);

                    IGrouping<DateTime, ResultExercise> resultLastTraining = CommandHandlerTools.Db.ResultsExercises
                                                                .Where(re => activeExercisesIDsInActiveDays.Contains(re.ExerciseId))
                                                                .OrderByDescending(re => re.DateTime)
                                                                .GroupBy(re => re.DateTime.Date)
                                                                .AsEnumerable()
                                                                .LastOrDefault();

                    information = GetInformationAboutLastExercises(resultLastTraining);
                    responseConverter = new ResponseTextConverter("Последняя тренировка:", information, "Выберите тренировочный день");
                    buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;

                case "Day":
                    IEnumerable<int> exercisesIDs = CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive)
                                                                                                                           .Select(d => d.Id);

                    IQueryable<ResultExercise> lastDateForExercises = CommandHandlerTools.Db.ResultsExercises.Where(re => exercisesIDs.Contains(re.ExerciseId))
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
                            lastResultsExercisesInCurrentDay = CommandHandlerTools.Db.ResultsExercises.Where(re => re.ExerciseId == resultExercise.ExerciseId && 
                                                                                         re.DateTime.Date == resultExercise.DateTime.Date);
                            isFirstQuery = false;
                        }
                        else
                            lastResultsExercisesInCurrentDay = lastResultsExercisesInCurrentDay.Union(CommandHandlerTools.Db.ResultsExercises.Where(re => re.ExerciseId == resultExercise.ExerciseId && 
                                                                                                                                re.DateTime.Date == resultExercise.DateTime.Date));
                    }

                    information = GetInformationAboutLastDay(lastResultsExercisesInCurrentDay);
                    responseConverter = new ResponseTextConverter("Последние результаты упражнений из этого дня:", information, "Выберите упражнение");
                    buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal WorkoutCH SaveResultsExerciseCommand()
        {
            CommandHandlerTools.Db.ResultsExercises.AddRange(CommandHandlerTools.CurrentUserContext.DataManager.ResultExercises);

            CommandHandlerTools.CurrentUserContext.DataManager.ResetResultExercises();

            CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseTextConverter responseConverter = new ResponseTextConverter("Введённые данные сохранены!", "Выберите упраженение");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        private string GetInformationAboutLastExercises(IEnumerable<ResultExercise>? resultExercises)
        {
            if (!resultExercises.Any())
                return "Нет информации для данного цикла";

            StringBuilder sb = new StringBuilder();

            ResultExercise firstResultExercise = resultExercises.First();

            IEnumerable<IGrouping<int, ResultExercise>> groupsResultsExercise = resultExercises.GroupBy(x => x.ExerciseId);

            sb.AppendLine($"Дата: {firstResultExercise.DateTime.ToShortDateString()}");

            foreach (IGrouping<int, ResultExercise> groupResultExercise in groupsResultsExercise)
            {
                ResultExercise firstGroupResultExercise = groupResultExercise.First();

                sb.AppendLine($"Упражнение: {firstGroupResultExercise.Exercise.Name.AddBold().AddQuotes()}");

                foreach (ResultExercise resultExercise in groupResultExercise)
                {
                    string resultExerciseStr = CommandHandlerTools.CurrentUserContext.DataManager.ConvertResultExerciseToString(resultExercise);

                    sb.AppendLine(resultExerciseStr);
                }
            }

            return sb.ToString().Trim();
        }

        private string GetInformationAboutLastDay(IEnumerable<ResultExercise>? resultExercises)
        {
            if (!resultExercises.HasItemsInCollection())
                return "Нет информации для данного дня";

            StringBuilder sb = new StringBuilder();

            IEnumerable<IGrouping<int, ResultExercise>> groupsResultsExercise = resultExercises.GroupBy(x => x.ExerciseId);

            foreach (IGrouping<int, ResultExercise> groupResultExercise in groupsResultsExercise)
            {
                ResultExercise firstResultExercise = groupResultExercise.First();

                sb.AppendLine($"Упражнение: {firstResultExercise.Exercise.Name.AddBold().AddQuotes()} | Дата: {firstResultExercise.DateTime.ToShortDateString().AddBold().AddQuotes()}");

                foreach (ResultExercise resultExercise in groupResultExercise)
                {
                    string resultExerciseStr = CommandHandlerTools.CurrentUserContext.DataManager.ConvertResultExerciseToString(resultExercise);

                    sb.AppendLine(resultExerciseStr);
                }
            }

            return sb.ToString().Trim();
        }
    }
}