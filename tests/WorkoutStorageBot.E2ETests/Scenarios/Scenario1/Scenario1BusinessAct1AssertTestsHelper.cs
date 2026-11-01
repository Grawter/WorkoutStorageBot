using FluentAssertions;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.E2ETests.Scenarios.Scenario1
{
    internal class Scenario1BusinessAct1AssertsTestsHelper : BaseAssertsBusinessActsAssertsHelper
    {
        private readonly string firstCycleName;

        private readonly string firstDayNameInFirstCycle;
        private readonly string secondDayNameInFirstCycle;

        private readonly string firstExerciseNameOnFirstDayInFirstCycle;
        private readonly string secondExerciseNameOnFirstDayInFirstCycle;
        private readonly string firstExerciseNameOnSecondDayInFirstCycle;
        private readonly string secondExerciseNameOnSecondDayInFirstCycle;

        internal Scenario1BusinessAct1AssertsTestsHelper(IContextKeeper contextKeeper,
                                                         EntityContext entityContext,
                                                         string userName,
                                                         long userID,
                                                         string firstName,
                                                         int userInformationID,

                                                         string firstCycleName,

                                                         string firstDayNameInFirstCycle,
                                                         string secondDayNameInFirstCycle,

                                                         string firstExerciseNameOnFirstDayInFirstCycle,
                                                         string secondExerciseNameOnFirstDayInFirstCycle,
                                                         string firstExerciseNameOnSecondDayInFirstCycle,
                                                         string secondExerciseNameOnSecondDayInFirstCycle) 
            : base (contextKeeper, 
                    entityContext,
                    userName,
                    userID,
                    firstName,
                    userInformationID)
        {
            this.firstCycleName = firstCycleName;

            this.firstDayNameInFirstCycle = firstDayNameInFirstCycle;
            this.secondDayNameInFirstCycle = secondDayNameInFirstCycle;

            this.firstExerciseNameOnFirstDayInFirstCycle = firstExerciseNameOnFirstDayInFirstCycle;
            this.secondExerciseNameOnFirstDayInFirstCycle = secondExerciseNameOnFirstDayInFirstCycle;
            this.firstExerciseNameOnSecondDayInFirstCycle = firstExerciseNameOnSecondDayInFirstCycle;
            this.secondExerciseNameOnSecondDayInFirstCycle = secondExerciseNameOnSecondDayInFirstCycle;
        }

        internal async Task AssertAct1(List<HandlerResult> handlerResults)
        {
            PrimaryHandlerResult handlerResult = (PrimaryHandlerResult)handlerResults.First();

            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            handlerResult.InformationSet.Should().NotBeNull();
            handlerResult.InformationSet.Message.Should().Be("Начнём");
            handlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.AddCycle, ButtonsSet.None));
            handlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            handlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            handlerResult.HasAccess.Should().BeTrue();
            handlerResult.IsNewContext.Should().BeTrue();
            handlerResult.IsNeedContinue.Should().BeFalse();
            handlerResult.IsNeedSendResponse.Should().BeTrue();
        }

        internal async Task AssertAct2(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Cycles.Any(x => x.UserInformationId == userID && x.Name == firstCycleName && x.IsActive == true).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddCycle);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be("Введите название тренировочного цикла");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct3(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Cycles.Any(x => x.UserInformationId == userID && x.Name == firstCycleName && x.IsActive == true).Should().BeTrue();
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == firstCycleName && x.Name == firstDayNameInFirstCycle).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddDays);
            userContext.ActiveCycle.Should().NotBeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Цикл {firstCycleName.AddBoldAndQuotes()} сохранён!
======================

Введите название тренирочного дня для цикла {firstCycleName.AddBoldAndQuotes()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct4(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == firstCycleName && x.Name == firstDayNameInFirstCycle).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"День {firstDayNameInFirstCycle.AddBoldAndQuotes()} сохранён в цикле {firstCycleName}!
======================
Введите название упражения для этого дня.
{CommonConsts.Exercise.ExamplesTypesExercise}
======================

{CommonConsts.Exercise.InputFormatExercise}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct5(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            userContext.DataManager.TempExercises.Should().HaveCount(1);
            DTOExercise? addedExrecise = userContext.DataManager.TempExercises.FirstOrDefault();
            addedExrecise.Should().NotBeNull();
            addedExrecise.Name.Should().Be(firstExerciseNameOnFirstDayInFirstCycle);
            addedExrecise.Day.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнение(я) зафиксировано(ы)!
======================

Введите след. упражнение(я) для дня {firstDayNameInFirstCycle.AddBoldAndQuotes()} либо нажмите ""Сохранить"" для сохранения зафиксированных упражнений
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveExercises, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct6(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Exercises.Should().HaveCount(0);

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            userContext.DataManager.TempExercises.Should().HaveCount(2);
            DTOExercise? firstAddedExrecise = userContext.DataManager.TempExercises.FirstOrDefault();
            firstAddedExrecise.Should().NotBeNull();
            firstAddedExrecise.Name.Should().Be(firstExerciseNameOnFirstDayInFirstCycle);
            firstAddedExrecise.Day.Should().BeNull();
            firstAddedExrecise.Mode.Should().Be(ExercisesMods.Count);

            DTOExercise? secondAddedExrecise = userContext.DataManager.TempExercises.Skip(1).FirstOrDefault();
            secondAddedExrecise.Should().NotBeNull();
            secondAddedExrecise.Name.Should().Be(secondExerciseNameOnFirstDayInFirstCycle);
            secondAddedExrecise.Day.Should().BeNull();
            secondAddedExrecise.Mode.Should().Be(ExercisesMods.WeightCount);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнение(я) зафиксировано(ы)!
======================

Введите след. упражнение(я) для дня {firstDayNameInFirstCycle.AddBoldAndQuotes()} либо нажмите ""Сохранить"" для сохранения зафиксированных упражнений
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveExercises, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct7(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempExercises.Should().BeNull();

            entityContext.Exercises.Should().HaveCount(2);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == firstDayNameInFirstCycle && x.Name == firstExerciseNameOnFirstDayInFirstCycle && x.Mode == ExercisesMods.Count)
                .Should().BeTrue();
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == firstDayNameInFirstCycle && x.Name == secondExerciseNameOnFirstDayInFirstCycle && x.Mode == ExercisesMods.WeightCount)
                .Should().BeTrue();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнения сохранены!
======================

Выберите дальнейшее действие
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct8(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Should().HaveCount(1);
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == firstCycleName && x.Name == secondDayNameInFirstCycle).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddDays);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Введите название тренирочного дня для цикла {firstCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct9(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Should().HaveCount(2);
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == firstCycleName && x.Name == secondDayNameInFirstCycle).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"День {secondDayNameInFirstCycle.AddBoldAndQuotes()} сохранён в цикле {firstCycleName}!
======================
Введите название упражения для этого дня.
{CommonConsts.Exercise.ExamplesTypesExercise}
======================

{CommonConsts.Exercise.InputFormatExercise}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct10(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Exercises.Should().HaveCount(2);

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            userContext.DataManager.TempExercises.Should().HaveCount(2);
            DTOExercise? firstAddedExrecise = userContext.DataManager.TempExercises.FirstOrDefault();
            firstAddedExrecise.Should().NotBeNull();
            firstAddedExrecise.Name.Should().Be(firstExerciseNameOnSecondDayInFirstCycle);
            firstAddedExrecise.Day.Should().BeNull();
            firstAddedExrecise.Mode.Should().Be(ExercisesMods.FreeResult);

            DTOExercise? secondAddedExrecise = userContext.DataManager.TempExercises.Skip(1).FirstOrDefault();
            secondAddedExrecise.Should().NotBeNull();
            secondAddedExrecise.Name.Should().Be(secondExerciseNameOnSecondDayInFirstCycle);
            secondAddedExrecise.Day.Should().BeNull();
            secondAddedExrecise.Mode.Should().Be(ExercisesMods.FreeResult);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнение(я) зафиксировано(ы)!
======================

Введите след. упражнение(я) для дня {secondDayNameInFirstCycle.AddBoldAndQuotes()} либо нажмите ""Сохранить"" для сохранения зафиксированных упражнений
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveExercises, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct11(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Start);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempExercises.Should().BeNull();

            entityContext.Exercises.Should().HaveCount(4);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == secondDayNameInFirstCycle && x.Name == firstExerciseNameOnSecondDayInFirstCycle && x.Mode == ExercisesMods.FreeResult)
                .Should().BeTrue();
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == secondDayNameInFirstCycle && x.Name == secondExerciseNameOnSecondDayInFirstCycle && x.Mode == ExercisesMods.FreeResult)
                .Should().BeTrue();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнения сохранены!
======================

Выберите дальнейшее действие
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct12(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Выберите интересующий раздел");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct13(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Выберите тренировочный день из цикла {firstCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct14(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Последняя тренировка:
======================
Не удалось получить результаты последней тренировки
======================

Выберите тренировочный день из цикла {firstCycleName.AddBoldAndQuotes()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct15(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindResultsByDate);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Введите дату искомой тренировки
======================

{CommonConsts.Exercise.FindResultsByDateFormat}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct16(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindResultsByDate);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Не удалось найти тренировки с датой '13.12.2000'
======================

Не удалось найти ближайших тренировок к искомой дате
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().HaveCount(1);

            KeyValuePair<string, string> parameter = updateHandlerResult.InformationSet.AdditionalParameters.First();
            parameter.Key.Should().Be("domainTypeForFind");
            parameter.Value.Should().Be("Day");

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct17(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Выберите тренировочный день из цикла {firstCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct18(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Выберите интересующий раздел");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }
    }
}