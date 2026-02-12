using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class WorkoutCH : CallBackCH
    {
        private static IReadOnlyDictionary<string, Func<WorkoutCH, Task<IInformationSet>>> commandMap
           = new Dictionary<string, Func<WorkoutCH, Task<IInformationSet>>>
           {
               { "Workout", (x) => Task.FromResult(x.WorkoutCommand()) },
               { "LastResults", (x) => x.LastResultsCommand() },
               { "StartFindResultsByDate", (x) => Task.FromResult(x.StartFindResultsByDateCommand()) },
               { "FindResultsByDate", (x) => x.FindResultsByDateCommand() },
               { "StartExerciseTimer", (x) => Task.FromResult(x.StartExerciseTimerCommand()) },
               { "StopExerciseTimer", (x) => Task.FromResult(x.StopExerciseTimerCommand()) },
               { "ShowExerciseTimer", (x) => Task.FromResult(x.ShowExerciseTimerCommand()) },
               { "ResetResultsExercise", (x) => Task.FromResult(x.ResetResultsExerciseCommand()) },
               { "SaveResultsExercise", (x) => x.SaveResultsExerciseCommand() },
           };

        internal WorkoutCH(CommandHandlerTools commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override async Task<IInformationSet> GetInformationSet()
        {
            Func<WorkoutCH, Task<IInformationSet>>? selectedCommand = commandMap.GetValueOrDefault(callbackQueryParser.SubDirection)
                ?? throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

            IInformationSet informationSet = await selectedCommand(this);

            CheckInformationSet(informationSet);

            return informationSet;
        }

        private IInformationSet WorkoutCommand()
        {
            this.CurrentUserContext.Navigation.ResetNavigation(); // not necessary, but just in case
            
            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Выберите тренировочный день");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> LastResultsCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            string information;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercises:
                    IEnumerable<int> activeDayIDs = this.CurrentUserContext.ActiveCycle.ThrowIfNull().Days.Where(d => !d.IsArchive)
                                                                                                          .Select(d => d.Id);

                    IQueryable<int> activeExercisesIDsInActiveDays = this.Db.Exercises.Where(e => !e.IsArchive && activeDayIDs.Contains(e.DayId))
                                                                                      .Select(e => e.Id);

                    var resultLastTraining = await this.Db.ResultsExercises
                                                                .AsNoTracking()
                                                                .Where(re => activeExercisesIDsInActiveDays.Contains(re.ExerciseId))
                                                                .Include(e => e.Exercise)
                                                                .GroupBy(re => re.DateTime.Date)
                                                                .Select(g => new
                                                                {
                                                                    Date = g.Key,
                                                                    Data = g.Select(x => x),
                                                                })
                                                                .OrderByDescending(x => x.Date)
                                                                .FirstOrDefaultAsync();


                    if (resultLastTraining == null)
                        information = "Не удалось получить результаты последней тренировки";
                    else
                        information = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastExercises(resultLastTraining.Date, resultLastTraining.Data);

                    responseTextBuilder = new ResponseTextBuilder("Последняя тренировка:", information, "Выберите тренировочный день");
                    buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    IEnumerable<int> exercisesIDs = this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Where(e => !e.IsArchive)
                                                                                                                          .Select(d => d.Id);
                    string dbProvider = this.Db.GetDBProvider();

                    string query;

                    if (dbProvider == "Microsoft.EntityFrameworkCore.Sqlite")
                    {
                        query = $@"
SELECT re.*
FROM [ResultsExercises] AS re
JOIN (
    SELECT ExerciseId, date(MAX(DateTime)) AS MaxDate
    FROM [ResultsExercises]
    WHERE ExerciseId IN ({string.Join(',', exercisesIDs)})
    GROUP BY ExerciseId
) last
ON last.ExerciseId = re.ExerciseId
AND date(re.DateTime) = last.MaxDate";
                    }
                    else
                        throw new NotImplementedException($"Операция не поддерживается для DBProvider {dbProvider}");

                    IEnumerable<ResultExercise> lastResultsExercisesInCurrentDay = await this.Db.ResultsExercises.FromSqlRaw(query)
                                                                                                                 .Include(r => r.Exercise)
                                                                                                                 .ToListAsync();

                    information = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastDay(lastResultsExercisesInCurrentDay);
                    responseTextBuilder = new ResponseTextBuilder("Последние результаты упражнений из этого дня:", information, "Выберите упражнение");
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

        private async Task<IInformationSet> FindResultsByDateCommand()
        {
            string findedDate = callbackQueryParser.GetRequiredAdditionalParameter(0);

            bool isNeedFindByCurrentDay = callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Exercise;

            IInformationSet informationSet = await SharedExercisesAndResultsLogicHelper.FindResultByDateCommand(this.Db, this.CurrentUserContext, findedDate, isNeedFindByCurrentDay);

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
                "Выберите упражнение");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> SaveResultsExerciseCommand()
        {
            foreach (DTOResultExercise tempResultsExercise in this.CurrentUserContext.DataManager.TempResultsExercise.ThrowIfNull())
            {
                tempResultsExercise.Exercise = this.CurrentUserContext.DataManager.CurrentExercise;
                this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().ResultsExercise.Add(tempResultsExercise);
            }
            await this.Db.AddEntities(this.CurrentUserContext.DataManager.TempResultsExercise);

            this.CurrentUserContext.DataManager.ResetTempResultsExercise();

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Введённые данные сохранены!", "Выберите упраженение");
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