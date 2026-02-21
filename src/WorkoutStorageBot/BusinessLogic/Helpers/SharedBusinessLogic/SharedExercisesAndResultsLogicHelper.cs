using Microsoft.EntityFrameworkCore;
using System.Text;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Exceptions;
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
    /// Повторяющаяся бизнес-логика, для упражнение и результатов упражнений которая может быть применима в разных CommandHandler
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

        internal static List<DTOExercise> GetExercisesFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new CreateExerciseException("Не удалось начать парсинг строки для создания упражнений, т.к. получена пустая строка");

            List<DTOExercise> exercises = new List<DTOExercise>();

            foreach (string exerciseWithType in text.Split(';', stringSplitOptions))
            {
                string[] exerciseAndType = exerciseWithType.Split('-', stringSplitOptions);

                string? name = exerciseAndType.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(name))
                    throw new CreateExerciseException("Не удалось получилось название упражнения.");

                if (!int.TryParse(exerciseAndType.Skip(1).FirstOrDefault(), out int type))
                    throw new CreateExerciseException($"Не удалось получить тип упражнения '{name}'");

                if (!Enum.IsDefined(typeof(ExercisesMods), type))
                    throw new CreateExerciseException($"Указанный тип ({type}) упражнения '{name}' не относится к допустимым");

                DTOExercise exercise = new DTOExercise() { Name = name, Mode = (ExercisesMods)type };
                exercises.Add(exercise);
            }

            return exercises;
        }

        internal static List<DTOResultExercise> GetResultsExerciseFromText(string text, ExercisesMods currentExerciseMode)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new CreateResultExerciseException("Не удалось начать парсинг строки для создания результатов упражнений, т.к. получена пустая строка");

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
                DTOResultExercise resultExercise = GetResultExerciseFromText(text.Trim(), currentExerciseMode);

                results.Add(resultExercise);
            }
            else
            {
                foreach (string resultExerciseStr in text.Split(separator, stringSplitOptions))
                {
                    DTOResultExercise resultExercise = GetResultExerciseFromText(resultExerciseStr.Trim(), currentExerciseMode);

                    results.Add(resultExercise);
                }
            }

            return results;
        }

        private static DTOResultExercise GetResultExerciseFromText(string resultExerciseStr, ExercisesMods currentExerciseMode)
        {
            string currentType = currentExerciseMode.ToString().AddQuotes();

            switch (currentExerciseMode)
            {
                case ExercisesMods.Count:

                    if (!int.TryParse(resultExerciseStr, out int singleCount))
                        throw new CreateResultExerciseException("Указанное количество повторений не является цифрой");

                    return new DTOResultExercise()
                    {
                        Count = singleCount,
                        DateTime = DateTime.Now,
                    };

                case ExercisesMods.WeightCount:

                    string[] resultExerciseWeightCount = resultExerciseStr.Split(" ", stringSplitOptions);

                    if (resultExerciseWeightCount.Length != 2)
                        throw new CreateResultExerciseException($"Некорректный ввод. Ожидаемое кол-во аргументов больше 2 для упражнения с типом {currentType}");

                    string weightStr = resultExerciseWeightCount.FirstOrDefault()
                        ?? throw new CreateResultExerciseException($"Не удалось получить указанный вес подхода для упражнения с типом {currentType}");

                    if (!float.TryParse(weightStr, out float weight))
                        throw new CreateResultExerciseException("Указанное значение веса подхода не является цифрой");

                    string countStr = resultExerciseWeightCount.Skip(1).FirstOrDefault()
                        ?? throw new CreateResultExerciseException($"Не удалось получить указанное кол-во повторений для упражнения с типом {currentType}");

                    if (!int.TryParse(countStr, out int count))
                        throw new CreateResultExerciseException("Указанное количество повторений не является цифрой");

                    return new DTOResultExercise()
                    {
                        Weight = weight,
                        Count = count,
                        DateTime = DateTime.Now,
                    };

                case ExercisesMods.FreeResult:
                    return new DTOResultExercise()
                    {
                        FreeResult = resultExerciseStr,
                        DateTime = DateTime.Now,
                    };

                default:
                    throw new CreateResultExerciseException($"Неподдерживаемый тип упражнения: {currentType}");

            }
        }

        internal static async Task<IInformationSet> FindResultByDateCommand(EntityContext db, UserContext userContext, string finderDate, bool isNeedFindByCurrentDay)
        {
            ResponseTextBuilder responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            if (!DateTime.TryParseExact(finderDate, CommonConsts.Exercise.ValidDateFormats, null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
            {
                responseConverter = new ResponseTextBuilder(
                    $"Не удалось получить дату из сообщения '{finderDate}', для корректного поиска придерживайтесь допустимого формата",
                    CommonConsts.Exercise.FindResultsByDateFormat,
                    "Введите дату искомой тренировки");

                buttonsSets = isNeedFindByCurrentDay
                    ? (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay)
                    : (ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout);

                informationSet = new MessageInformationSet(responseConverter.Build(), buttonsSets);

                return informationSet;
            }

            List<int> exercisesIDs = GetNotArchivingExercisesIDs(userContext, isNeedFindByCurrentDay);

            IEnumerable<ResultExercise> resultLastTraining = await db.ResultsExercises.AsNoTracking()
                                                                                        .Where(re => exercisesIDs.Contains(re.ExerciseId)
                                                                                                && re.DateTime.Date == dateTime)
                                                                                        .Include(e => e.Exercise)
                                                                                        .ToListAsync();
            if (resultLastTraining.HasItemsInCollection())
            {
                userContext.Navigation.ResetMessageNavigationTarget();

                string information = GetInformationAboutLastExercises(dateTime, resultLastTraining);

                string target = isNeedFindByCurrentDay
                    ? $"Выберите упражнение из дня {userContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({userContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})"
                    : $"Выберите тренировочный день из цикла {userContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}";

                responseConverter = new ResponseTextBuilder($"Найденная тренировка:", information, target);

                buttonsSets = isNeedFindByCurrentDay
                    ? (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout)
                    : (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

                informationSet = new MessageInformationSet(responseConverter.Build(), buttonsSets);

                return informationSet;
            }

            ResultExercise? resultLessThanFinderDate = await db.ResultsExercises.AsNoTracking()
                                                                                    .Where(re =>
                                                                                            exercisesIDs.Contains(re.ExerciseId)
                                                                                            && re.DateTime.Date < dateTime)
                                                                                    .OrderByDescending(re => re.DateTime)
                                                                                    .FirstOrDefaultAsync();

            DateTime trainingDateLessThanFinderDate = resultLessThanFinderDate?.DateTime ?? DateTime.MinValue;

            ResultExercise? resultDateGreaterThanFinderDate = await db.ResultsExercises.AsNoTracking()
                                                                                        .Where(re =>
                                                                                                exercisesIDs.Contains(re.ExerciseId)
                                                                                                && re.DateTime.Date > dateTime)
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

            responseConverter = new ResponseTextBuilder($"Не удалось найти тренировки с датой '{finderDate}'",
                    hasClosestByDateTrainings
                    ? "Найдены ближайшие тренировки к искомой дате:"
                    : "Не удалось найти ближайших тренировок к искомой дате");

            buttonsSets = (ButtonsSet.FoundResultsByDate, isNeedFindByCurrentDay
                ? ButtonsSet.ExercisesListWithLastWorkoutForDay
                : ButtonsSet.DaysListWithLastWorkout);

            informationSet = new MessageInformationSet(responseConverter.Build(), buttonsSets, additionalParameters);

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