using WorkoutStorageBot.BusinessLogic.Context.StepStore;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class CommonCH : CallBackCH
    {
        private static IReadOnlyDictionary<string, Func<CommonCH, Task<IInformationSet>>> commandMap
            = new Dictionary<string, Func<CommonCH, Task<IInformationSet>>>
            {
                { "Back", (x) => Task.FromResult(x.BackCommand()) },
                { "ToMain", (x) => Task.FromResult(x.ToMainCommand()) },
            };

        internal CommonCH(CommandHandlerTools commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override async Task<IInformationSet> GetInformationSet()
        {
            Func<CommonCH, Task<IInformationSet>>? selectedCommand = commandMap.GetValueOrDefault(callbackQueryParser.SubDirection)
                ?? throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");

            IInformationSet informationSet = await selectedCommand(this);

            CheckInformationSet(informationSet);

            return informationSet;
        }

        private IInformationSet BackCommand()
        {
            string buttonsSetStr = callbackQueryParser.AdditionalParameters.First();

            if (!buttonsSetStr.TryParseToEnum(out ButtonsSet previousButtonsSet))
                throw new InvalidOperationException($"Не удалось получить информацию о шаге по buttonsSetStr = '{buttonsSetStr}'");

            StepInformation previousStep = StepStorage.GetStep(previousButtonsSet);

            this.CurrentUserContext.Navigation.SetQueryFrom(previousStep.QueryFrom);
            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            string target;

            switch(previousStep.ButtonsSet)
            {
                case ButtonsSet.DaysListWithLastWorkout:
                case ButtonsSet.SettingCycle:
                case ButtonsSet.SettingDays:
                case ButtonsSet.DaysList:
                    target = $"{previousStep.Message} {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}";
                    break;

                case ButtonsSet.ExercisesListWithLastWorkoutForDay:
                case ButtonsSet.SettingDay:
                case ButtonsSet.SettingExercises:
                case ButtonsSet.ExercisesList:
                    target = $"{previousStep.Message} {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()} ({this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})";
                    break;

                case ButtonsSet.SettingExercise:
                    target = $"{previousStep.Message} {this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()} ({this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBold()}-{this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBold()})";
                    break;

                default:
                    target = previousStep.Message;
                break;
            }

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(target);

            (ButtonsSet, ButtonsSet) buttonsSets = (previousStep.ButtonsSet, previousStep.BackButtonsSet);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ToMainCommand()
        {
            this.CurrentUserContext.DataManager.ResetAll();

            StepInformation mainStep = StepStorage.GetMainStep();

            this.CurrentUserContext.Navigation.SetQueryFrom(mainStep.QueryFrom);
            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(mainStep.Message);
            (ButtonsSet, ButtonsSet) buttonsSets = (mainStep.ButtonsSet, mainStep.BackButtonsSet);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }
    }
}