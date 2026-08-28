using Microsoft.EntityFrameworkCore;
using System.Text;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic
{
    /// <summary>
    /// Повторяющаяся бизнес-логика, для упражнений и результатов упражнений, которая может быть применима в разных CommandHandler
    /// </summary>
    internal static class SharedExercisesAndResultsLogicHelper
    {
        private static StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

        internal static IEnumerable<int> GetAllUserExercisesIds(UserContext userContext)
        {
            List<DTOCycle> cycles = userContext.UserInformation.Cycles;

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

        internal static IQueryable<ResultExercise> GetAllUserResultsExercises(EntityContext db, UserContext userContext)
        {
            IEnumerable<int> userExercisesIds = GetAllUserExercisesIds(userContext);

            IQueryable<ResultExercise> resultsExercises = db.ResultsExercises.AsNoTracking()
                                                                             .Where(x => userExercisesIds.Contains(x.ExerciseId));

            return resultsExercises;
        }

        internal static (List<DTOExercise> Exercises, string? Error) GetExercisesFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (new List<DTOExercise>(), "Не удалось начать парсинг строки для создания упражнений, т.к. получена пустая строка");

            List<DTOExercise> exercises = new List<DTOExercise>();

            foreach (string exerciseWithType in text.Split(';', stringSplitOptions))
            {
                string[] exerciseAndType = exerciseWithType.Split('-', stringSplitOptions);

                string? name = exerciseAndType.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(name))
                    return (new List<DTOExercise>(), "Не удалось получилось название упражнения.");

                if (!int.TryParse(exerciseAndType.Skip(1).FirstOrDefault(), out int type))
                    return (new List<DTOExercise>(), $"Не удалось получить тип упражнения '{name}'");

                if (!Enum.IsDefined(typeof(ExercisesMods), type))
                    return (new List<DTOExercise>(), $"Указанный тип ({type}) упражнения '{name}' не относится к допустимым");

                DTOExercise exercise = new DTOExercise() { Name = name, Mode = (ExercisesMods)type };
                exercises.Add(exercise);
            }

            return (exercises, null);
        }

        internal static (List<DTOResultExercise> ResultsExercise, string? Error) GetResultsExerciseFromText(string text, ExercisesMods currentExerciseMode)
        {
            if (string.IsNullOrWhiteSpace(text))
                return(new List<DTOResultExercise>(), "Не удалось начать парсинг строки для создания результатов упражнений, т.к. получена пустая строка");

            List<DTOResultExercise> results = new();

            StringSplitOptions stringSplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            string separator = currentExerciseMode switch
            {
                ExercisesMods.Count => " ",
                ExercisesMods.WeightCount => ";",
                _ => string.Empty
            };

            if (currentExerciseMode == ExercisesMods.FreeResult)
            {
                (DTOResultExercise ResultExercise, string? Error) result = GetResultExerciseFromText(text.Trim(), currentExerciseMode);

                if (string.IsNullOrWhiteSpace(result.Error))
                    results.Add(result.ResultExercise);
            }
            else
            {
                foreach (string resultExerciseStr in text.Split(separator, stringSplitOptions))
                {
                    (DTOResultExercise ResultExercise, string? Error) result = GetResultExerciseFromText(resultExerciseStr.Trim(), currentExerciseMode);

                    if (!string.IsNullOrWhiteSpace(result.Error))
                        return (new List<DTOResultExercise>(), result.Error);

                    results.Add(result.ResultExercise);
                }
            }

            return (results, null);
        }

        private static (DTOResultExercise ResultExercise, string? Error) GetResultExerciseFromText(string resultExerciseStr, ExercisesMods currentExerciseMode)
        {
            string currentType = currentExerciseMode.ToString().AddQuotes();

            DTOResultExercise DTOResultExercise = new DTOResultExercise();

            switch (currentExerciseMode)
            {
                case ExercisesMods.Count:

                    if (!int.TryParse(resultExerciseStr, out int singleCount))
                        return(DTOResultExercise, "Указанное количество повторений не является цифрой");

                    DTOResultExercise.Count = singleCount;
                    DTOResultExercise.DateTime = DateTime.Now;

                    return (DTOResultExercise, null);

                case ExercisesMods.WeightCount:

                    string[] resultExerciseWeightCount = resultExerciseStr.Split(" ", stringSplitOptions);

                    if (resultExerciseWeightCount.Length != 2)
                        return (DTOResultExercise, $"Некорректный ввод. Ожидаемое кол-во аргументов больше 2 для упражнения с типом {currentType}");

                    string? weightStr = resultExerciseWeightCount.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(weightStr))
                        return (DTOResultExercise, $"Не удалось получить указанный вес подхода для упражнения с типом {currentType}");

                    if (!float.TryParse(weightStr, out float weight))
                        return (DTOResultExercise, "Указанное значение веса подхода не является цифрой");

                    string? countStr = resultExerciseWeightCount.Skip(1).FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(countStr))
                        return (DTOResultExercise, $"Не удалось получить указанное кол-во повторений для упражнения с типом {currentType}");

                    if (!int.TryParse(countStr, out int count))
                        return(DTOResultExercise, "Указанное количество повторений не является цифрой");

                    DTOResultExercise.Weight = weight;
                    DTOResultExercise.Count = count;
                    DTOResultExercise.DateTime = DateTime.Now;

                    return (DTOResultExercise, null);

                case ExercisesMods.FreeResult:

                    DTOResultExercise.FreeResult = resultExerciseStr;
                    DTOResultExercise.DateTime = DateTime.Now;

                    return (DTOResultExercise, null);

                default:
                    return (DTOResultExercise, $"Неподдерживаемый тип упражнения: {currentType}");
            }
        }

        internal static async Task<IInformationSet> GetInformationSetWithTextResultsByDateCommand(EntityContext db, UserContext userContext, DateTime finderDate, bool isNeedFindByCurrentDay)
        {
            IInformationSet informationSet;

            List<int> exercisesIDs = GetNotArchivingExercisesIDs(userContext, isNeedFindByCurrentDay);

            IEnumerable<ResultExercise> resultsLastTraining = await GetGetResultsExerciseByDate(db, finderDate, exercisesIDs);

            if (!resultsLastTraining.HasItemsInCollection())
            {
                informationSet = await GetInformationSetWithClosestByDateTrainings(db, finderDate, exercisesIDs, isNeedFindByCurrentDay);

                return informationSet;
            }

            userContext.Navigation.ResetMessageNavigationTarget();

            string information = GetInformationAboutLastExercises(finderDate, resultsLastTraining);

            string target = isNeedFindByCurrentDay
                ? $"Выберите упражнение из дня {userContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({userContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})"
                : $"Выберите тренировочный день из цикла {userContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}";

            ResponseTextBuilder responseConverter = new ResponseTextBuilder($"Найденная тренировка:", information, target);

            (ButtonsSet, ButtonsSet) buttonsSets = isNeedFindByCurrentDay
                ? (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout)
                : (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Build(), buttonsSets);

            return informationSet;
        }

        private static async Task<IEnumerable<ResultExercise>> GetGetResultsExerciseByDate(EntityContext db, UserContext userContext, DateTime finderDate, bool isNeedFindByCurrentDay)
        {
            List<int> exercisesIDs = GetNotArchivingExercisesIDs(userContext, isNeedFindByCurrentDay);

            return await GetGetResultsExerciseByDate(db, finderDate, exercisesIDs);
        }

        private static async Task<IEnumerable<ResultExercise>> GetGetResultsExerciseByDate(EntityContext db, DateTime finderDate, List<int> exercisesIDs)
        {
            IEnumerable<ResultExercise> resultsLastTraining = await db.ResultsExercises.AsNoTracking()
                                                                                        .Where(re => exercisesIDs.Contains(re.ExerciseId)
                                                                                                && re.DateTime.Date == finderDate)
                                                                                        .Include(e => e.Exercise)
                                                                                        .ToListAsync();

            return resultsLastTraining;
        }

        private static async Task<IInformationSet> GetInformationSetWithClosestByDateTrainings(EntityContext db, DateTime finderDate, List<int> exercisesIDs, bool isNeedFindByCurrentDay)
        {
            ResultExercise? resultLessThanFinderDate = await db.ResultsExercises.AsNoTracking()
                                                                                    .Where(re =>
                                                                                            exercisesIDs.Contains(re.ExerciseId)
                                                                                            && re.DateTime.Date < finderDate)
                                                                                    .OrderByDescending(re => re.DateTime)
                                                                                    .FirstOrDefaultAsync();

            DateTime trainingDateLessThanFinderDate = resultLessThanFinderDate?.DateTime ?? DateTime.MinValue;

            ResultExercise? resultDateGreaterThanFinderDate = await db.ResultsExercises.AsNoTracking()
                                                                                        .Where(re =>
                                                                                                exercisesIDs.Contains(re.ExerciseId)
                                                                                                && re.DateTime.Date > finderDate)
                                                                                        .OrderBy(re => re.DateTime)
                                                                                        .FirstOrDefaultAsync();

            DateTime trainingDateGreaterThanFinderDate = resultDateGreaterThanFinderDate?.DateTime ?? DateTime.MinValue;

            bool hasClosestByDateTrainings = false;

            Dictionary<string, string> additionalParameters = new Dictionary<string, string>();

            if (trainingDateLessThanFinderDate > DateTime.MinValue)
            {
                additionalParameters.Add(nameof(trainingDateLessThanFinderDate), trainingDateLessThanFinderDate.ToString(CommonConsts.Common.DateFormat));
                hasClosestByDateTrainings = true;
            }

            if (trainingDateGreaterThanFinderDate > DateTime.MinValue)
            {
                additionalParameters.Add(nameof(trainingDateGreaterThanFinderDate), trainingDateGreaterThanFinderDate.ToString(CommonConsts.Common.DateFormat));
                hasClosestByDateTrainings = true;
            }

            additionalParameters.Add("domainTypeForFind", isNeedFindByCurrentDay
                    ? CommonConsts.DomainsAndEntities.Exercise
                    : CommonConsts.DomainsAndEntities.Day);

            ResponseTextBuilder responseConverter = new ResponseTextBuilder($"Не удалось найти тренировки с датой '{finderDate.ToString(CommonConsts.Common.DateFormat)}'",
                    hasClosestByDateTrainings
                    ? "Найдены ближайшие тренировки к искомой дате:"
                    : "Не удалось найти ближайших тренировок к искомой дате");

            (ButtonsSet, ButtonsSet)  buttonsSets = (ButtonsSet.FoundResultsByDate, isNeedFindByCurrentDay
                ? ButtonsSet.ExercisesListWithLastWorkoutForDay
                : ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Build(), buttonsSets, additionalParameters);

            return informationSet;
        }

        private static List<int> GetNotArchivingExercisesIDs(UserContext userContext, bool isNeedFindByCurrentDay)
        {
            List<int> exercisesIDs;

            if (isNeedFindByCurrentDay)
            {
                exercisesIDs = userContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Where(e => !e.IsArchive)
                                                                                         .Select(d => d.Id)
                                                                                         .ToList();
            }
            else
            {
                exercisesIDs = userContext.ActiveCycle.ThrowIfNull().Days.Where(d => !d.IsArchive)
                                                                         .SelectMany(d => d.Exercises)
                                                                         .Where(e => !e.IsArchive)
                                                                         .Select(e => e.Id)
                                                                         .ToList();
            }

            return exercisesIDs;

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

                sb.AppendLine($"Упражнение: {firstGroupResultExercise.Exercise.ThrowIfNull().Name.AddBoldAndQuotes()}");

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

            string dateStr = string.Empty;

            foreach (IGrouping<int, ResultExercise> groupResultExercise in groupsResultsExercise)
            {
                ResultExercise firstResultExercise = groupResultExercise.First();

                string tempDateStr = firstResultExercise.DateTime.ToString(CommonConsts.Common.DateFormat).AddBoldAndQuotes();

                if (dateStr != tempDateStr)
                {
                    dateStr = tempDateStr;

                    sb.AppendLine($"Дата: {tempDateStr}");
                }

                sb.AppendLine($"Упражнение: {firstResultExercise.Exercise.ThrowIfNull().Name.AddBoldAndQuotes()}");

                foreach (ResultExercise resultExercise in groupResultExercise)
                {
                    string resultExerciseStr = ConvertResultExerciseToString(resultExercise);

                    sb.AppendLine(resultExerciseStr);
                }
            }

            return sb.ToString().Trim();
        }

        private static string ConvertResultExerciseToString(ResultExercise resultExercise)
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
                throw new InvalidOperationException($"Не удалось отобразить данные для результата упражнения с ID: {resultExercise.Id}, ID упражнения: {resultExercise.ExerciseId}, тип упражнения: {resultExercise.Exercise.ThrowIfNull().Mode.ToString().AddQuotes()}");
        }
    }
}