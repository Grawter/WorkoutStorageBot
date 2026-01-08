using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.SharedCommandHandler;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.BusinessLogicHelpers;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.Entities.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using Microsoft.EntityFrameworkCore;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class WorkoutCH : CallBackCH
    {
        internal WorkoutCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override IInformationSet GetInformationSet()
        {
            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Workout":
                    informationSet = WorkoutCommand();
                    break;

                case "LastResults":
                    informationSet = LastResultsCommand();
                    break;

                case "StartFindResultsByDate":
                    informationSet = StartFindResultsByDateCommand();
                    break;

                case "FindResultsByDate":
                    informationSet = FindResultsByDateCommand();
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
                    informationSet = SaveResultsExerciseCommand();                     
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

        private IInformationSet LastResultsCommand()
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

                    var resultLastTraining = this.CommandHandlerTools.Db.ResultsExercises
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
                                                                .FirstOrDefault();


                    if (resultLastTraining == null)
                        throw new InvalidOperationException("Не удалось получить результаты последней тренировки");

                    information = WorkoutDataHelper.GetInformationAboutLastExercises(resultLastTraining.Date, resultLastTraining.Data);
                    responseConverter = new ResponseTextConverter("Последняя тренировка:", information, "Выберите тренировочный день");
                    buttonsSets = (ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    IEnumerable<int> exercisesIDs = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises.Where(e => !e.IsArchive)
                                                                                                                                .Select(d => d.Id);

                    IEnumerable<IGrouping<DateTime, ResultExercise>> lastDateForExercises = this.CommandHandlerTools.Db.ResultsExercises
                                                .AsNoTracking()
                                                .Where(re => exercisesIDs.Contains(re.ExerciseId))
                                                .GroupBy(re => re.ExerciseId)
                                                //order Data in group and get first (older) element for get lastDate
                                                .Select(reGROUP => reGROUP.OrderByDescending(re => re.DateTime.Date).First())
                                                .ToList()
                                                .GroupBy(re => re.DateTime.Date);

                    IQueryable<ResultExercise> lastResultsExercisesInCurrentDay = this.CommandHandlerTools.Db.ResultsExercises.AsNoTracking()
                                                                                                                              .Where(e => false);
                    
                    foreach (IGrouping<DateTime, ResultExercise> resultExercise in lastDateForExercises)
                    {
                        IEnumerable<int> groupExercisesIDs = resultExercise.Select(x => x.ExerciseId);

                        lastResultsExercisesInCurrentDay = lastResultsExercisesInCurrentDay.Union(this.CommandHandlerTools.Db.ResultsExercises.AsNoTracking()
                                                                                                                                              .Where(re => 
                                                                                                                        groupExercisesIDs.Contains(re.ExerciseId) &&
                                                                                                                        re.DateTime.Date == resultExercise.Key))
                                                                                                                                              .Include(e => e.Exercise);
                    }

                    information = WorkoutDataHelper.GetInformationAboutLastDay(lastResultsExercisesInCurrentDay);
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

        private IInformationSet FindResultsByDateCommand()
        {
            SharedCH sharedCH = new SharedCH(this.CommandHandlerTools);

            string findedDate = callbackQueryParser.GetRequiredAdditionalParameter(0);

            bool isNeedFindByCurrentDay = callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Exercise;

            IInformationSet informationSet = sharedCH.FindResultByDateCommand(findedDate, isNeedFindByCurrentDay);

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

        private IInformationSet SaveResultsExerciseCommand()
        {
            foreach (DTOResultExercise tempResultsExercise in this.CommandHandlerTools.CurrentUserContext.DataManager.TempResultsExercise)
            {
                tempResultsExercise.Exercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise;
                this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ResultsExercise.Add(tempResultsExercise);
            }
            this.CommandHandlerTools.Db.AddEntities(this.CommandHandlerTools.CurrentUserContext.DataManager.TempResultsExercise);

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