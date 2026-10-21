using FluentAssertions;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

namespace WorkoutStorageBot.E2ETests.Scenarios.Scenario1
{
    internal class Scenario1BusinessAct4AssertsTestsHelper : BaseAssertsBusinessActsAssertsHelper
    {
        private readonly string secondCycleName;

        private readonly string firstDayNameInSecondCycle;
        private readonly string secondDayNameInSecondCycle;

        private readonly string firstExerciseNameOnFirstDayInSecondCycle;
        private readonly string secondExerciseNameOnFirstDayInSecondCycle;
        private readonly string firstExerciseNameOnSecondDayInSecondCycle;
        private readonly string secondExerciseNameOnSecondDayInSecondCycle;

        internal Scenario1BusinessAct4AssertsTestsHelper(IContextKeeper contextKeeper,
                                                         EntityContext entityContext,
                                                         string userName,
                                                         long userID,
                                                         string firstName,
                                                         int userInformationID,

                                                         string secondCycleName,

                                                         string firstDayNameInSecondCycle,
                                                         string secondDayNameInSecondCycle,

                                                         string firstExerciseNameOnFirstDayInSecondCycle,
                                                         string secondExerciseNameOnFirstDayInSecondCycle,
                                                         string firstExerciseNameOnSecondDayInSecondCycle,
                                                         string secondExerciseNameOnSecondDayInSecondCycle)
            : base (contextKeeper, 
                    entityContext,
                    userName,
                    userID,
                    firstName,
                    userInformationID)
        {
            this.secondCycleName = secondCycleName;

            this.firstDayNameInSecondCycle = firstDayNameInSecondCycle;
            this.secondDayNameInSecondCycle = secondDayNameInSecondCycle;

            this.firstExerciseNameOnFirstDayInSecondCycle = firstExerciseNameOnFirstDayInSecondCycle;
            this.secondExerciseNameOnFirstDayInSecondCycle = secondExerciseNameOnFirstDayInSecondCycle;
            this.firstExerciseNameOnSecondDayInSecondCycle = firstExerciseNameOnSecondDayInSecondCycle;
            this.secondExerciseNameOnSecondDayInSecondCycle = secondExerciseNameOnSecondDayInSecondCycle;
        }

        internal async Task AssertAct1(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующие настройки");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Settings, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct2(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующие настройки для циклов");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycles, ButtonsSet.Settings));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct3(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующий цикл");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct4(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующую настройку для цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.CycleList));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct5(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Активный цикл изменён на {secondCycleName.AddBoldAndQuotes()}
======================

Выберите интересующую настройку для цикла {secondCycleName.AddBoldAndQuotes()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycle, ButtonsSet.SettingCycles));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct6(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Выберите интересующий раздел");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct7(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите тренировочный день из цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct8(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите упражнение из дня {firstDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct9(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Фиксирование результатов упражнения {firstExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
======================
{CommonConsts.ResultExercise.InputFormatExerciseResultCount}
======================

Введите результат(ы) подхода(ов)
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct10(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Should().HaveCount(4);
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnFirstDayInSecondCycle && x.Count == 20).Should().BeFalse();
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnFirstDayInSecondCycle && x.Count == 30).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            userContext.DataManager.TempResultsExercise.Should().HaveCount(2);
            userContext.DataManager.TempResultsExercise.First().Count.Should().Be(20);
            userContext.DataManager.TempResultsExercise.Skip(1).First().Count.Should().Be(30);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Подход(ы) зафиксирован(ы)
======================

Введите результат след. подхода для упражения {firstExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
либо нажмите ""Сохранить"" для сохранения указанных подходов
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveResultsExercise, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct11(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Should().HaveCount(6);
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnFirstDayInSecondCycle && x.Count == 20).Should().BeTrue();
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnFirstDayInSecondCycle && x.Count == 30).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempResultsExercise.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Введённые данные сохранены!
======================

Выберите упражнение
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
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
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Последние результаты упражнений из этого дня:
======================
Дата: {DateTime.Now.ToString(CommonConsts.Common.DateFormat).AddBoldAndQuotes()}
Упражнение: {firstExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (20)
Повторения: (30)
======================

Выберите упражнение из дня {firstDayNameInSecondCycle.AddBold()} ({secondCycleName.AddBold()})
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct13(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Фиксирование результатов упражнения {secondExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
======================
{CommonConsts.ResultExercise.InputFormatExerciseResultFreeResult}
======================

Введите результат(ы) подхода(ов)
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);

        }

        internal async Task AssertAct14(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == secondExerciseNameOnFirstDayInSecondCycle && x.FreeResult == "TestValue221").Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            userContext.DataManager.TempResultsExercise.Should().HaveCount(1);
            userContext.DataManager.TempResultsExercise.First().FreeResult.Should().Be("TestValue221");

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Подход(ы) зафиксирован(ы)
======================

Введите результат след. подхода для упражения {secondExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
либо нажмите ""Сохранить"" для сохранения указанных подходов
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveResultsExercise, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct15(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == secondExerciseNameOnFirstDayInSecondCycle && x.FreeResult == "TestValue221").Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempResultsExercise.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Введённые данные сохранены!
======================

Выберите упражнение
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct16(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindResultsByDateInDay);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Введите дату искомой тренировки
======================

{CommonConsts.Exercise.FindResultsByDateFormat}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

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
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Найденная тренировка:
======================
Дата: {DateTime.Now.ToString(CommonConsts.Common.DateFormat)}
Упражнение: {firstExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (20)
Повторения: (30)
Упражнение: {secondExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
=> TestValue221
======================

Выберите упражнение из дня {firstDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
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
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Выберите тренировочный день из цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct19(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Выберите упражнение из дня {secondDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct20(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Фиксирование результатов упражнения {firstExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
======================
{CommonConsts.ResultExercise.InputFormatExerciseResultWeightCount}
======================

Введите результат(ы) подхода(ов)
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct21(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Should().HaveCount(7);
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnSecondDayInSecondCycle && x.Weight == 20 && x.Count == 40).Should().BeFalse();
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnSecondDayInSecondCycle && x.Weight == 30 && x.Count == 60).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            userContext.DataManager.TempResultsExercise.Should().HaveCount(2);
            userContext.DataManager.TempResultsExercise.First().Weight.Should().Be(20);
            userContext.DataManager.TempResultsExercise.First().Count.Should().Be(40);

            userContext.DataManager.TempResultsExercise.Skip(1).First().Weight.Should().Be(30);
            userContext.DataManager.TempResultsExercise.Skip(1).First().Count.Should().Be(60);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Подход(ы) зафиксирован(ы)
======================

Введите результат след. подхода для упражения {firstExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
либо нажмите ""Сохранить"" для сохранения указанных подходов
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveResultsExercise, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct22(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Should().HaveCount(9);
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnSecondDayInSecondCycle && x.Weight == 20 && x.Count == 40).Should().BeTrue();
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == firstExerciseNameOnSecondDayInSecondCycle && x.Weight == 30 && x.Count == 60).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempResultsExercise.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Введённые данные сохранены!
======================

Выберите упражнение
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct23(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Последние результаты упражнений из этого дня:
======================
Дата: {DateTime.Now.ToString(CommonConsts.Common.DateFormat).AddBoldAndQuotes()}
Упражнение: {firstExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (40) => Вес: (20)
Повторения: (60) => Вес: (30)
======================

Выберите упражнение из дня {secondDayNameInSecondCycle.AddBold()} ({secondCycleName.AddBold()})
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct24(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Фиксирование результатов упражнения {secondExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
======================
{CommonConsts.ResultExercise.InputFormatExerciseResultFreeResult}
======================

Введите результат(ы) подхода(ов)
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct25(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == secondExerciseNameOnSecondDayInSecondCycle && x.FreeResult == "TestValue222").Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddResultForExercise);

            userContext.DataManager.TempResultsExercise.Should().HaveCount(1);
            userContext.DataManager.TempResultsExercise.First().FreeResult.Should().Be("TestValue222");

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Подход(ы) зафиксирован(ы)
======================

Введите результат след. подхода для упражения {secondExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
либо нажмите ""Сохранить"" для сохранения указанных подходов
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveResultsExercise, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct26(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Name == secondExerciseNameOnSecondDayInSecondCycle && x.FreeResult == "TestValue222").Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempResultsExercise.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Введённые данные сохранены!
======================

Выберите упражнение
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct27(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindResultsByDateInDay);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Введите дату искомой тренировки
======================

{CommonConsts.Exercise.FindResultsByDateFormat}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct28(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Найденная тренировка:
======================
Дата: {DateTime.Now.ToString(CommonConsts.Common.DateFormat)}
Упражнение: {firstExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (40) => Вес: (20)
Повторения: (60) => Вес: (30)
Упражнение: {secondExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
=> TestValue222
======================

Выберите упражнение из дня {secondDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct29(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Выберите тренировочный день из цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct30(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Последняя тренировка:
======================
Дата: {DateTime.Now.ToString(CommonConsts.Common.DateFormat)}
Упражнение: {firstExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (20)
Повторения: (30)
Упражнение: {secondExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
=> TestValue221
Упражнение: {firstExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (40) => Вес: (20)
Повторения: (60) => Вес: (30)
Упражнение: {secondExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
=> TestValue222
======================

Выберите тренировочный день из цикла {secondCycleName.AddBoldAndQuotes()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct31(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.FindResultsByDate);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Введите дату искомой тренировки
======================

{CommonConsts.Exercise.FindResultsByDateFormat}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.DaysListWithLastWorkout));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct32(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Найденная тренировка:
======================
Дата: {DateTime.Now.ToString(CommonConsts.Common.DateFormat)}
Упражнение: {firstExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (20)
Повторения: (30)
Упражнение: {secondExerciseNameOnFirstDayInSecondCycle.AddBoldAndQuotes()}
=> TestValue221
Упражнение: {firstExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
Повторения: (40) => Вес: (20)
Повторения: (60) => Вес: (30)
Упражнение: {secondExerciseNameOnSecondDayInSecondCycle.AddBoldAndQuotes()}
=> TestValue222
======================

Выберите тренировочный день из цикла {secondCycleName.AddBoldAndQuotes()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct33(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Выберите интересующий раздел");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }
    }
}