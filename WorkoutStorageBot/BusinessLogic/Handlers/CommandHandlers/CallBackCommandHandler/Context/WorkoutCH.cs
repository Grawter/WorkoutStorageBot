#region using
using System.Text;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
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
                    IEnumerable<int> dayIDs = CommandHandlerTools.CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive).Select(d => d.Id);

                    IQueryable<Exercise> exercises = CommandHandlerTools.Db.Exercises.Where(e => !e.IsArchive && dayIDs.Contains(e.DayId));

                    IGrouping<DateTime, ResultExercise> resultLastTraining = CommandHandlerTools.Db.ResultsExercises
                                                                .Where(re => exercises.Select(e => e.Id).Contains(re.ExerciseId))
                                                                .OrderByDescending(re => re.DateTime)
                                                                .GroupBy(re => re.DateTime)
                                                                .AsEnumerable()
                                                                .LastOrDefault();

                    information = GetInformationAboutLastExercises(exercises, resultLastTraining);
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

                    information = GetInformationAboutLastDay(CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises, lastResultsExercisesInCurrentDay);
                    responseConverter = new ResponseTextConverter("Последняя результаты упражнений из этого дня:", information, "Выберите упражнение");
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

        private static string GetInformationAboutLastExercises(IEnumerable<Exercise>? exercises, IEnumerable<ResultExercise>? resultExercises)
        {
            if (!exercises.Any() || !resultExercises.Any())
                return "Нет информации для данного цикла";

            var resultQuery = exercises
                        .Join(resultExercises,
                            e => e.Id,
                            rE => rE.ExerciseId,
                            (e, rE) => new { e.Name, rE.Count, rE.Weight, rE.DateTime })
                        .GroupBy(rE => rE.Name).ToArray();


            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Дата: {resultExercises.First().DateTime.ToShortDateString()}");

            for (int i = 0; i < resultQuery.Count(); i++)
            {
                sb.AppendLine($"Упражнение: {resultQuery[i].Key}");

                foreach (var result in resultQuery[i])
                {
                    sb.AppendLine($"({result.Count}) => ({result.Weight})");
                }
            }

            return sb.ToString().Trim();
        }

        private static string GetInformationAboutLastDay(IEnumerable<Exercise> exercises, IEnumerable<ResultExercise>? resultExercises)
        {
            if (!exercises.Any() || !resultExercises.Any())
                return "Нет информации для данного цикла";

            var resultQuery = exercises
                        .Join(resultExercises,
                            e => e.Id,
                            rE => rE.ExerciseId,
                            (e, rE) => new { e.Name, rE.Count, rE.Weight, rE.DateTime })
                        .GroupBy(rE => rE.Name).ToArray();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < resultQuery.Count(); i++)
            {
                sb.AppendLine($"Упражнение: {resultQuery[i].Key} | Дата: {resultQuery[i].First().DateTime.ToShortDateString()}");

                foreach (var result in resultQuery[i])
                {
                    sb.AppendLine($"({result.Count}) => ({result.Weight})");
                }
            }

            return sb.ToString().Trim();
        }
    }
}