using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class WorkoutCH : CallBackCH
    {
        private static IReadOnlyDictionary<string, Func<WorkoutCH, CancellationToken, Task<IInformationSet>>> commandMap
           = new Dictionary<string, Func<WorkoutCH, CancellationToken, Task<IInformationSet>>>
           {
               { "Workout", (x, y) => Task.FromResult(x.WorkoutCommand()) },
               { "LastResults", (x, y) => x.LastResultsCommand(y) },
               { "StartFindResultsByDate", (x, y) => Task.FromResult(x.StartFindResultsByDateCommand()) },
               { "FindResultsByDate", (x, y) => x.FindResultsByDateCommand(y) },
               { "StartExerciseTimer", (x, y) => Task.FromResult(x.StartExerciseTimerCommand()) },
               { "StopExerciseTimer", (x, y) => Task.FromResult(x.StopExerciseTimerCommand()) },
               { "ShowExerciseTimer", (x, y) => Task.FromResult(x.ShowExerciseTimerCommand()) },
               { "ResetResultsExercise", (x, y) => Task.FromResult(x.ResetResultsExerciseCommand()) },
               { "SaveResultsExercise", (x, y) => x.SaveResultsExerciseCommand(y) },
           };

        internal WorkoutCH(CommandHandlerTools commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        protected override async Task<IInformationSet> GetInformationSet(CancellationToken cancellationToken)
        {
            Func<WorkoutCH, CancellationToken, Task<IInformationSet>> selectedCommand = GetSelectedCommand();

            IInformationSet informationSet = await selectedCommand(this, cancellationToken);

            CheckInformationSet(informationSet);

            return informationSet;
        }

        protected Func<WorkoutCH, CancellationToken, Task<IInformationSet>> GetSelectedCommand()
            => commandMap.GetValueOrDefault(callbackQueryParser.SubDirection)
                ?? throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

        private IInformationSet WorkoutCommand()
        {
            this.CurrentUserContext.Navigation.ResetNavigation(); // not necessary, but just in case

            // immediately first set the active cycle as current
            this.CurrentUserContext.DataManager.SetCurrentDomain(this.CurrentUserContext.ActiveCycle.ThrowIfNull());

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"Выберите тренировочный день или посмотрите предыдущие результаты из цикла {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> LastResultsCommand(CancellationToken cancellationToken)
        {
            ResponseTextBuilder responseTextBuilder;
            string information;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercises:
                    IEnumerable<int> activeDayIDs = this.CurrentUserContext.ActiveCycle.ThrowIfNull().Days.Where(d => !d.IsArchive)
                                                                                                          .Select(d => d.Id);

                    List<int> activeExercisesIDsInActiveDays = await this.Db.Exercises.Where(e => !e.IsArchive && activeDayIDs.Contains(e.DayId))
                                                                                      .Select(e => e.Id)
                                                                                      .ToListAsync(cancellationToken);

                    // DateTime? потому что MaxAsync возвращает null, если у пользователя еще нет результатов по активным упражнения.
                    // Это именно прикол LINQ to Entities. В SQL функция MAX(column) над пустым множеством возвращает NULL.
                    DateTime? lastDate = await this.Db.ResultsExercises.Where(re => activeExercisesIDsInActiveDays.Contains(re.ExerciseId))
                                                                       .MaxAsync(re => (DateTime?)re.DateTime.Date, cancellationToken);
                    if (lastDate == null)
                        information = "Не удалось получить результаты последней тренировки";
                    else
                    {
                        List<ResultExercise> lastTrainingResults = await this.Db.ResultsExercises.AsNoTracking()
                                                                                             .Where(re => activeExercisesIDsInActiveDays.Contains(re.ExerciseId) &&
                                                                                                    re.DateTime.Date == lastDate
                                                                                              )
                                                                                             .Include(e => e.Exercise)
                                                                                             .ToListAsync(cancellationToken);

                        information = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastExercises(lastDate.Value, lastTrainingResults);
                    }                   

                    responseTextBuilder = new ResponseTextBuilder("Последняя тренировка:", 
                                                                  information, 
                                                                  $"Выберите тренировочный день или посмотрите предыдущие результаты из цикла {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    List<int> exercisesIDs = this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Where(e => !e.IsArchive)
                                                                                                                   .Select(d => d.Id)
                                                                                                                   .ToList();
                    IEnumerable<ResultExercise> lastResultsExercisesInCurrentDay;

                    if (exercisesIDs.Count > 0)
                    {
                        lastResultsExercisesInCurrentDay = await this.Db.ResultsExercises.AsNoTracking()
                                                                                         .Where(re => exercisesIDs.Contains(re.ExerciseId))
                                                                                         .Where(re =>
                                                                                             // Берем только те записи, дата которых совпадает с максимальной датой для этого упражнения
                                                                                             this.Db.ResultsExercises
                                                                                                 .Where(x => x.ExerciseId == re.ExerciseId)
                                                                                                 .Max(x => x.DateTime).Date == re.DateTime.Date
                                                                                         )
                                                                                         .Include(r => r.Exercise)
                                                                                         .ToListAsync(cancellationToken);
                    }
                    else
                        lastResultsExercisesInCurrentDay = new List<ResultExercise>();

                    information = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastDay(lastResultsExercisesInCurrentDay);
                    responseTextBuilder = new ResponseTextBuilder("Последние результаты упражнений из этого дня:", 
                                                                  information, 
                                                                  $"Выберите упражнение или посмотрите предыдущие результаты дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBold()} ({this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
                    buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet StartFindResultsByDateCommand()
        {
            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"Введите дату искомой тренировки", CommonConsts.Exercise.FindResultsByDateFormat);
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercises:

                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.FindResultsByDate);

                    buttonsSets = (ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout); 

                    break;

                case CommonConsts.DomainsAndEntities.Day:

                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.FindResultsByDateInDay);

                    buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);

                    break;

                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> FindResultsByDateCommand(CancellationToken cancellationToken)
        {
            string finderDate = callbackQueryParser.GetRequiredAdditionalParameter(0);

            bool isNeedFindByCurrentDay = callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Exercise;

            string target = isNeedFindByCurrentDay
                    ? $"Выберите упражнение или посмотрите предыдущие результаты дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})"
                    : $"Выберите тренировочный день или посмотрите предыдущие результаты из цикла {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}";
         
            IInformationSet informationSet;

            if (!DateTime.TryParseExact(finderDate, CommonConsts.Exercise.ValidDateFormats, null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
            {
                (ButtonsSet, ButtonsSet) buttonsSets = isNeedFindByCurrentDay
                    ? (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay)
                    : (ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout);

                ResponseTextBuilder responseConverter = new ResponseTextBuilder($"Не удалось найти данные, т.к. не удалось распарсить дату '{finderDate}'", target);

                informationSet = new MessageInformationSet(responseConverter.Build(), buttonsSets);

                return informationSet;
            }

            informationSet = await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(this.Db, 
                                                                                                                      this.CurrentUserContext, 
                                                                                                                      dateTime, 
                                                                                                                      isNeedFindByCurrentDay,
                                                                                                                      cancellationToken);

            return informationSet;
        }

        private IInformationSet StartExerciseTimerCommand()
        {
            this.CurrentUserContext.DataManager.SetCurrentDateTimeToExerciseTimer();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Таймер запущен");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.FixExerciseTimer, ButtonsSet.None);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet StopExerciseTimerCommand()
        {
            string timerValue = GetTimerValue();

            DTOResultExercise resultExercise = new DTOResultExercise() { FreeResult = timerValue, DateTime = DateTime.Now};

            this.CurrentUserContext.DataManager.AddTempResultsExercise([resultExercise]);

            this.CurrentUserContext.DataManager.ResetExerciseTimer();

            this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddCommentForExerciseTimer);

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"Результат: {timerValue.AddBold()}", 
                "Если требуется, введите комментарий к результату или выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.SaveResultsExercise, ButtonsSet.None);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ShowExerciseTimerCommand()
        {
            string timerValue = GetTimerValue();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"С момента запуска таймера прошло: {timerValue.AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.FixExerciseTimer, ButtonsSet.None);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ResetResultsExerciseCommand()
        {
            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            this.CurrentUserContext.DataManager.ResetTempResultsExercise();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"Результат упражнения '{this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}' был сброшен",
                $"Выберите упражнение или посмотрите предыдущие результаты дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBold()} ({this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> SaveResultsExerciseCommand(CancellationToken cancellationToken)
        {
            foreach (DTOResultExercise tempResultsExercise in this.CurrentUserContext.DataManager.TempResultsExercise.ThrowIfNull())
            {
                tempResultsExercise.Exercise = this.CurrentUserContext.DataManager.CurrentExercise;
                this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.Add(tempResultsExercise);
            }
            await this.Db.AddEntities(this.CurrentUserContext.DataManager.TempResultsExercise, cancellationToken);

            this.CurrentUserContext.DataManager.ResetTempResultsExercise();

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Введённые данные сохранены", 
                $"Выберите упражнение или посмотрите предыдущие результаты дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }
        
        private string GetTimerValue()
        {
            DateTime currentTime = DateTime.Now;

            TimeSpan timerResult = currentTime.Subtract(this.CurrentUserContext.DataManager.ExerciseTimer);

            string timerResultStr = $"{timerResult.ToString(CommonConsts.Common.TimeFormatTimeSpan)}";

            return timerResultStr;
        }
    }
}