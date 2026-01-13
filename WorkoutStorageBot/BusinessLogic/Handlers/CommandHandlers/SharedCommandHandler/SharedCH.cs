using Microsoft.EntityFrameworkCore;
using System.Text;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.SharedCommandHandler
{
    /// <summary>
    /// Логика, которая может быть применима в разных CommandHandler
    /// </summary>
    internal class SharedCH : CommandHandler
    {
        internal SharedCH(CommandHandlerData commandHandlerTools) : base(commandHandlerTools)
        {
        }

        internal override Task<IInformationSet> GetInformationSet()
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<int> GetAllUserExercisesIds()
        {
            List<DTOCycle> cycles = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles;

            foreach (DTOCycle cycle in cycles)
            {
                foreach (DTODay day in cycle.Days)
                {
                    foreach (DTOExercise exercise in day.Exercises)
                    {
                        yield return exercise.Id;
                    }
                }
            }
        }

        internal IQueryable<ResultExercise> GetAllUserResultsExercises()
        {
            IEnumerable<int> userExercisesIds = GetAllUserExercisesIds();

            IQueryable<ResultExercise> resultsExercises = this.CommandHandlerTools.Db.ResultsExercises.AsNoTracking()
                                                                                                      .Where(x => userExercisesIds.Contains(x.ExerciseId));

            return resultsExercises;
        }

        internal MessageInformationSet GetAccessDeniedMessageInformationSet()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter("Отказано в действии");

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Main, ButtonsSet.None);

            return new MessageInformationSet(responseConverter.Convert(), buttonsSets);
        }

        internal async Task<IInformationSet> FindResultByDateCommand(string findedDate, bool isNeedFindByCurrentDay)
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            Dictionary<string, string> additionalParameters = new Dictionary<string, string>();

            if (!DateTime.TryParseExact(findedDate, CommonConsts.Exercise.ValidDateFormats, null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
            {
                responseConverter = new ResponseTextConverter(
                    $"Не удалось получить дату из сообщения '{findedDate}', для корректного поиска придерживайтесь допустимого формата",
                    CommonConsts.Exercise.FindResultsByDateFormat,
                    "Введите дату искомой тренировки");

                buttonsSets = isNeedFindByCurrentDay 
                    ? (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay)
                    : (ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout);
            }
            else
            {

                IEnumerable<int> exercisesIDs;

                if (isNeedFindByCurrentDay)
                {
                    exercisesIDs = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive)
                                                                                                               .Select(d => d.Id);
                }
                else
                {
                    exercisesIDs = this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive)
                                                                                               .SelectMany(d => d.Exercises)
                                                                                               .Where(e => !e.IsArchive)
                                                                                               .Select(e => e.Id);
                }

                IEnumerable<ResultExercise> resultLastTraining = await this.CommandHandlerTools.Db.ResultsExercises.AsNoTracking()
                                                                                                            .Where(re => exercisesIDs.Contains(re.ExerciseId)
                                                                                                                   && re.DateTime.Date == dateTime)
                                                                                                            .Include(e => e.Exercise)
                                                                                                            .ToListAsync();
                if (resultLastTraining.HasItemsInCollection())
                {
                    this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    string information = GetInformationAboutLastExercises(dateTime, resultLastTraining);

                    responseConverter = new ResponseTextConverter($"Найденная тренировка:", information, "Выберите тренировочный день");
                    
                    buttonsSets = isNeedFindByCurrentDay 
                        ? (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout)
                        : (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                }
                else
                {
                    ResultExercise? resultLessThanFindedDate = await this.CommandHandlerTools.Db.ResultsExercises.AsNoTracking()
                                                                                                          .Where(re =>
                                                                                                                    exercisesIDs.Contains(re.ExerciseId)
                                                                                                                    && re.DateTime.Date < dateTime)
                                                                                                          .OrderByDescending(re => re.DateTime)
                                                                                                          .FirstOrDefaultAsync();

                    DateTime trainingDateLessThanFindedDate = resultLessThanFindedDate?.DateTime ?? DateTime.MinValue;

                    ResultExercise? resultDateGreaterThanFindedDate = await this.CommandHandlerTools.Db.ResultsExercises.AsNoTracking()
                                                                                                             .Where(re =>
                                                                                                                        exercisesIDs.Contains(re.ExerciseId)
                                                                                                                        && re.DateTime.Date > dateTime)
                                                                                                             .OrderBy(re => re.DateTime)
                                                                                                             .FirstOrDefaultAsync();

                    DateTime trainingDateGreaterThanFindedDate = resultDateGreaterThanFindedDate?.DateTime ?? DateTime.MinValue;

                    bool hasClosestByDateTrainings = false;

                    if (trainingDateLessThanFindedDate > DateTime.MinValue)
                    {
                        additionalParameters.Add(nameof(trainingDateLessThanFindedDate), trainingDateLessThanFindedDate.ToString(CommonConsts.Common.DateFormat));
                        hasClosestByDateTrainings = true;
                    }

                    if (trainingDateGreaterThanFindedDate > DateTime.MinValue)
                    {
                        additionalParameters.Add(nameof(trainingDateGreaterThanFindedDate), trainingDateGreaterThanFindedDate.ToString(CommonConsts.Common.DateFormat));
                        hasClosestByDateTrainings = true;
                    }

                    additionalParameters.Add("domainTypeForFind", isNeedFindByCurrentDay 
                            ? CommonConsts.DomainsAndEntities.Exercise 
                            : CommonConsts.DomainsAndEntities.Day);

                    responseConverter = new ResponseTextConverter($"Не удалось найти тренировки с датой '{findedDate}'",
                         hasClosestByDateTrainings
                         ? "Найдены ближайшие тренировки к искомой дате:"
                         : "Не удалось найти ближайших тренировок к искомой дате");

                    buttonsSets = (ButtonsSet.FoundResultsByDate, isNeedFindByCurrentDay 
                        ? ButtonsSet.ExercisesListWithLastWorkoutForDay
                        : ButtonsSet.DaysListWithLastWorkout);
                }
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return informationSet;
        }

        internal static string GetInformationAboutLastExercises(DateTime filterDateTime, IEnumerable<ResultExercise>? resultsExercises)
        {
            if (!resultsExercises.HasItemsInCollection())
                return "Нет информации для данного цикла";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Дата: {filterDateTime.ToString(CommonConsts.Common.DateFormat)}");

            IEnumerable<IGrouping<int, ResultExercise>> groupsResultsExercise = resultsExercises.GroupBy(x => x.ExerciseId);

            foreach (IGrouping<int, ResultExercise> groupResultExercise in groupsResultsExercise)
            {
                ResultExercise firstGroupResultExercise = groupResultExercise.First();

                sb.AppendLine($"Упражнение: {firstGroupResultExercise.Exercise.Name.AddBoldAndQuotes()}");

                foreach (ResultExercise resultExercise in groupResultExercise)
                {
                    string resultExerciseStr = ConvertResultExerciseToString(resultExercise);

                    sb.AppendLine(resultExerciseStr);
                }
            }

            return sb.ToString().Trim();
        }

        internal static string GetInformationAboutLastDay(IEnumerable<ResultExercise>? resultsExercises)
        {
            if (!resultsExercises.HasItemsInCollection())
                return "Нет информации для данного дня";

            StringBuilder sb = new StringBuilder();

            IEnumerable<IGrouping<int, ResultExercise>> groupsResultsExercise = resultsExercises.GroupBy(x => x.ExerciseId);

            foreach (IGrouping<int, ResultExercise> groupResultExercise in groupsResultsExercise)
            {
                ResultExercise firstResultExercise = groupResultExercise.First();

                sb.AppendLine($"Упражнение: {firstResultExercise.Exercise.Name.AddBoldAndQuotes()} | Дата: {firstResultExercise.DateTime.ToString(CommonConsts.Common.DateFormat).AddBoldAndQuotes()}");

                foreach (ResultExercise resultExercise in groupResultExercise)
                {
                    string resultExerciseStr = ConvertResultExerciseToString(resultExercise);

                    sb.AppendLine(resultExerciseStr);
                }
            }

            return sb.ToString().Trim();
        }

        internal static string ConvertResultExerciseToString(ResultExercise resultExercise)
        {
            if (!string.IsNullOrWhiteSpace(resultExercise.FreeResult))
                return $"=> {resultExercise.FreeResult}";
            else if (resultExercise.Count.HasValue)
            {
                if (resultExercise.Weight.HasValue)
                    return $"Повторения: ({resultExercise.Count}) => Вес: ({resultExercise.Weight})";
                else
                    return $"Повторения: ({resultExercise.Count})";
            }
            else
                throw new InvalidOperationException($"Не удалось отобразить данные для результата упражнения с ID: {resultExercise.Id}, ID упражнения: {resultExercise.ExerciseId}, тип упражнения: {resultExercise.Exercise.Mode.ToString().AddQuotes()}");
        }
    }
}