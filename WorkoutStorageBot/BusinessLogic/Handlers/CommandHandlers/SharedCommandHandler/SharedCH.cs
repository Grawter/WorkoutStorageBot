#region using
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.BusinessLogicHelpers;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.DomainsAndEntities;
using WorkoutStorageBot.Model.HandlerData;
#endregion

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

        internal override CommandHandler Expectation(params HandlerAction[] handlerActions)
        {
            throw new NotImplementedException();
        }

        internal override IInformationSet GetData()
        {
            if (this.InformationSet == null)
                throw new InvalidOperationException($"Из '{nameof(SharedCH)}' вернулся пустой {nameof(this.InformationSet)}");

            return this.InformationSet;
        }

        internal IEnumerable<int> GetAllUserExercisesIds()
        {
            List<Cycle> cycles = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles;

            foreach (Cycle cycle in cycles)
            {
                foreach (Day day in cycle.Days)
                {
                    foreach (Exercise exercise in day.Exercises)
                    {
                        yield return exercise.Id;
                    }
                }
            }
        }

        internal IQueryable<ResultExercise> GetAllUserResultsExercises()
        {
            IEnumerable<int> userExercisesIds = GetAllUserExercisesIds();

            IQueryable<ResultExercise> resultsExercises = this.CommandHandlerTools.Db.ResultsExercises.Where(x => userExercisesIds.Contains(x.ExerciseId));

            return resultsExercises;
        }

        internal MessageInformationSet GetAccessDeniedMessageInformationSet()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter("Отказано в действии");

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Main, ButtonsSet.None);

            return new MessageInformationSet(responseConverter.Convert(), buttonsSets);
        }

        internal SharedCH FindResultByDateCommand(string findedDate, bool isNeedFindByCurrentDay)
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

                IQueryable<int> exercisesIDs;

                if (isNeedFindByCurrentDay)
                {
                    exercisesIDs = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive)
                                                                                                               .Select(d => d.Id)
                                                                                                               .AsQueryable();
                }
                else
                {
                    IEnumerable<int> activeDayIDs = this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive)
                                                                                                                .Select(d => d.Id);

                    exercisesIDs = this.CommandHandlerTools.Db.Exercises.Where(e => !e.IsArchive && activeDayIDs.Contains(e.DayId))
                                                                        .Select(e => e.Id);
                }

                IQueryable<ResultExercise> resultLastTraining = this.CommandHandlerTools.Db.ResultsExercises.Where(re => exercisesIDs.Contains(re.ExerciseId)
                                                                                                                   && re.DateTime.Date == dateTime);
                if (resultLastTraining.HasItemsInCollection())
                {
                    this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

                    string information = WorkoutDataHelper.GetInformationAboutLastExercises(resultLastTraining);

                    responseConverter = new ResponseTextConverter($"Найденная тренировка:", information, "Выберите тренировочный день");
                    
                    buttonsSets = isNeedFindByCurrentDay 
                        ? (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout)
                        : (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                }
                else
                {
                    DateTime trainingDateLessThanFindedDate = this.CommandHandlerTools.Db.ResultsExercises.Where(re =>
                                                                                                                    exercisesIDs.Contains(re.ExerciseId)
                                                                                                                    && re.DateTime.Date < dateTime)
                                                                                                          .OrderByDescending(re => re.DateTime)
                                                                                                          .FirstOrDefault()?.DateTime ?? DateTime.MinValue;

                    DateTime trainingDateGreaterThanFindedDate = this.CommandHandlerTools.Db.ResultsExercises.Where(re =>
                                                                                                                        exercisesIDs.Contains(re.ExerciseId)
                                                                                                                        && re.DateTime.Date > dateTime)
                                                                                                             .OrderBy(re => re.DateTime)
                                                                                                             .FirstOrDefault()?.DateTime ?? DateTime.MinValue;

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

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return this;
        }
    }
}