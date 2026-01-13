using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.SharedCommandHandler;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Model.Entities.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Extensions;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class WorkoutCH : CallBackCH
    {
        internal WorkoutCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override async Task<IInformationSet> GetInformationSet()
        {
            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Workout":
                    informationSet = WorkoutCommand();
                    break;

                case "LastResults":
                    informationSet = await LastResultsCommand();
                    break;

                case "StartFindResultsByDate":
                    informationSet = StartFindResultsByDateCommand();
                    break;

                case "FindResultsByDate":
                    informationSet = await FindResultsByDateCommand();
                    break;

                case "StartExerciseTimer":
                    informationSet = StartExerciseTimerCommand();
                    break;

                case "StopExerciseTimer":
                    informationSet = StopExerciseTimerCommand();
                    break;

                case "ResetResultsExercise":
                    informationSet = ResetResultsExerciseCommand();
                    break;

                case "SaveResultsExercise":
                    informationSet = await SaveResultsExerciseCommand();                     
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            CheckInformationSet(informationSet); 

            return informationSet;
        }

        private IInformationSet WorkoutCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetNavigation(); // not necessary, but just in case
            
            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите тренировочный день");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> LastResultsCommand()
        {
            ResponseTextConverter responseConverter;
            string information;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercises:
                    IEnumerable<int> activeDayIDs = this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Days.Where(d => !d.IsArchive)
                                                                                                                .Select(d => d.Id);

                    IQueryable<int> activeExercisesIDsInActiveDays = this.CommandHandlerTools.Db.Exercises.Where(e => !e.IsArchive && activeDayIDs.Contains(e.DayId))
                                                                                                          .Select(e => e.Id);

                    var resultLastTraining = await this.CommandHandlerTools.Db.ResultsExercises
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
                        throw new InvalidOperationException("Не удалось получить результаты последней тренировки");

                    information = SharedCH.GetInformationAboutLastExercises(resultLastTraining.Date, resultLastTraining.Data);
                    responseConverter = new ResponseTextConverter("Последняя тренировка:", information, "Выберите тренировочный день");
                    buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    IEnumerable<int> exercisesIDs = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive)
                                                                                                                                .Select(d => d.Id);
                    string dbProvider = this.CommandHandlerTools.Db.GetDBProvider();

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

                    IEnumerable<ResultExercise> lastResultsExercisesInCurrentDay = await this.CommandHandlerTools.Db.ResultsExercises.FromSqlRaw(query)
                                                                                                                                     .Include(r => r.Exercise)
                                                                                                                                     .ToListAsync();

                    information = SharedCH.GetInformationAboutLastDay(lastResultsExercisesInCurrentDay);
                    responseConverter = new ResponseTextConverter("Последние результаты упражнений из этого дня:", information, "Выберите упражнение");
                    buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet StartFindResultsByDateCommand()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter($"Введите дату искомой тренировки", CommonConsts.Exercise.FindResultsByDateFormat);
            string information;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercises:

                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.FindResultsByDate);

                    buttonsSets = (ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout); 

                    break;

                case CommonConsts.DomainsAndEntities.Day:

                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.FindResultsByDateInDay);

                    buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);

                    break;

                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.DomainType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> FindResultsByDateCommand()
        {
            SharedCH sharedCH = new SharedCH(this.CommandHandlerTools);

            string findedDate = callbackQueryParser.GetRequiredAdditionalParameter(0);

            bool isNeedFindByCurrentDay = callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Exercise;

            IInformationSet informationSet = await sharedCH.FindResultByDateCommand(findedDate, isNeedFindByCurrentDay);

            return informationSet;
        }

        private IInformationSet StartExerciseTimerCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.DataManager.StartExerciseTimer();

            ResponseTextConverter responseConverter = new ResponseTextConverter("Таймер запущен");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.FixExerciseTimer, ButtonsSet.None);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet StopExerciseTimerCommand()
        {
            string timerValue = GetTimerValue();

            DTOResultExercise resultExercise = new DTOResultExercise() { FreeResult = timerValue, DateTime = DateTime.Now};

            this.CommandHandlerTools.CurrentUserContext.DataManager.AddTempResultsExercise([resultExercise]);

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetExerciseTimer();

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddCommentForExerciseTimer);

            ResponseTextConverter responseConverter = new ResponseTextConverter($"Результат: {timerValue.AddBold()}", 
                "Если требуется, введите комментарий к результату или выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.SaveResultsExercise, ButtonsSet.None);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ResetResultsExerciseCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetTempResultsExercise();

            ResponseTextConverter responseConverter = new ResponseTextConverter($"Результат упражнения '{this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}' был сброшен", 
                "Выберите упражнение");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> SaveResultsExerciseCommand()
        {
            foreach (DTOResultExercise tempResultsExercise in this.CommandHandlerTools.CurrentUserContext.DataManager.TempResultsExercise)
            {
                tempResultsExercise.Exercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise;
                this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ResultsExercise.Add(tempResultsExercise);
            }
            await this.CommandHandlerTools.Db.AddEntities(this.CommandHandlerTools.CurrentUserContext.DataManager.TempResultsExercise);

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetTempResultsExercise();

            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextConverter responseConverter = new ResponseTextConverter("Введённые данные сохранены!", "Выберите упраженение");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }
        
        private string GetTimerValue()
        {
            DateTime currentTime = DateTime.Now;

            TimeSpan timerResult = currentTime.Subtract(this.CommandHandlerTools.CurrentUserContext.DataManager.ExerciseTimer);

            string timerResultStr = $"{timerResult.ToString(CommonConsts.Common.TimeFormatTimeSpan)}";

            return timerResultStr;
        }
    }
}