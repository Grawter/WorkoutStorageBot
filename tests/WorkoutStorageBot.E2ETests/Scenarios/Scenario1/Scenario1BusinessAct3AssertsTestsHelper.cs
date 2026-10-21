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
    internal class Scenario1BusinessAct3AssertsTestsHelper : BaseAssertsBusinessActsAssertsHelper
    {
        private readonly string secondCycleName;

        private readonly string firstDayNameInSecondCycle;
        private readonly string secondDayNameInSecondCycle;

        private readonly string firstExerciseNameOnFirstDayInSecondCycle;
        private readonly string secondExerciseNameOnFirstDayInSecondCycle;
        private readonly string firstExerciseNameOnSecondDayInSecondCycle;
        private readonly string secondExerciseNameOnSecondDayInSecondCycle;

        internal Scenario1BusinessAct3AssertsTestsHelper(IContextKeeper contextKeeper,
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
            updateHandlerResult.InformationSet.Message.Should().Be("Выберите интересующие настройки");
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
            updateHandlerResult.InformationSet.Message.Should().Be("Выберите интересующие настройки для циклов");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingCycles, ButtonsSet.Settings));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct3(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Cycles.Should().HaveCount(1);
            entityContext.Cycles.Any(x => x.UserInformationId == userID && x.Name == secondCycleName && x.IsActive == false).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddCycle);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be("Введите название тренировочного цикла");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingCycles));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct4(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Cycles.Should().HaveCount(2);
            entityContext.Cycles.Any(x => x.UserInformationId == userID && x.Name == secondCycleName && x.IsActive == false).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Цикл {secondCycleName.AddBoldAndQuotes()} сохранён!
======================

Выберите дальнейшие действия
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.AddDays, ButtonsSet.SettingCycles));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct5(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == firstDayNameInSecondCycle).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddDays);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Введите название тренирочного дня для цикла {(secondCycleName.AddBoldAndQuotes())}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct6(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == firstDayNameInSecondCycle).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"День {firstDayNameInSecondCycle.AddBoldAndQuotes()} сохранён в цикле {secondCycleName}!");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.AddExercises, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct7(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Введите название(я) и тип(ы) упражнения(й) для дня {firstDayNameInSecondCycle.AddBoldAndQuotes()}
======================
{CommonConsts.Exercise.ExamplesTypesExercise}
======================

{CommonConsts.Exercise.InputFormatExercise}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercises));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct8(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Exercises.Should().HaveCount(4);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == firstDayNameInSecondCycle && x.Name == firstExerciseNameOnFirstDayInSecondCycle && x.Mode == ExercisesMods.Count)
                .Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            userContext.DataManager.TempExercises.Should().HaveCount(1);
            DTOExercise? addedExrecise = userContext.DataManager.TempExercises.FirstOrDefault();
            addedExrecise.Should().NotBeNull();
            addedExrecise.Name.Should().Be(firstExerciseNameOnFirstDayInSecondCycle);
            addedExrecise.Day.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнение(я) зафиксировано(ы)!
======================

Введите след. упражнение(я) для дня {firstDayNameInSecondCycle.AddBoldAndQuotes()} либо нажмите ""Сохранить"" для сохранения зафиксированных упражнений
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveExercises, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct9(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempExercises.Should().BeNull();

            entityContext.Exercises.Should().HaveCount(5);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == firstDayNameInSecondCycle && x.Name == firstExerciseNameOnFirstDayInSecondCycle && x.Mode == ExercisesMods.Count)
                .Should().BeTrue();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнения сохранены!
======================

Выберите интересующие настройки для упражнений из дня {firstDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct10(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Введите название(я) и тип(ы) упражнения(й) для дня {firstDayNameInSecondCycle.AddBoldAndQuotes()}
======================
{CommonConsts.Exercise.ExamplesTypesExercise}
======================

{CommonConsts.Exercise.InputFormatExercise}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercises));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct11(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Exercises.Should().HaveCount(5);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == firstDayNameInSecondCycle && x.Name == secondExerciseNameOnFirstDayInSecondCycle && x.Mode == ExercisesMods.FreeResult)
                .Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            userContext.DataManager.TempExercises.Should().HaveCount(1);
            DTOExercise? addedExrecise = userContext.DataManager.TempExercises.FirstOrDefault();
            addedExrecise.Should().NotBeNull();
            addedExrecise.Name.Should().Be(secondExerciseNameOnFirstDayInSecondCycle);
            addedExrecise.Day.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнение(я) зафиксировано(ы)!
======================

Введите след. упражнение(я) для дня {firstDayNameInSecondCycle.AddBoldAndQuotes()} либо нажмите ""Сохранить"" для сохранения зафиксированных упражнений
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveExercises, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct12(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempExercises.Should().BeNull();

            entityContext.Exercises.Should().HaveCount(6);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == firstDayNameInSecondCycle && x.Name == secondExerciseNameOnFirstDayInSecondCycle && x.Mode == ExercisesMods.FreeResult)
                .Should().BeTrue();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнения сохранены!
======================

Выберите интересующие настройки для упражнений из дня {firstDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct13(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Выберите интересующие настройки для дней из цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDays, ButtonsSet.SettingCycle));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct14(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == secondDayNameInSecondCycle).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddDays);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Введите название тренирочного дня для цикла {(secondCycleName.AddBoldAndQuotes())}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct15(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == secondDayNameInSecondCycle).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"День {secondDayNameInSecondCycle.AddBoldAndQuotes()} сохранён в цикле {secondCycleName}!");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.AddExercises, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct16(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Введите название(я) и тип(ы) упражнения(й) для дня {secondDayNameInSecondCycle.AddBoldAndQuotes()}
======================
{CommonConsts.Exercise.ExamplesTypesExercise}
======================

{CommonConsts.Exercise.InputFormatExercise}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingExercises));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct17(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Exercises.Should().HaveCount(6);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == secondDayNameInSecondCycle && x.Name == firstExerciseNameOnSecondDayInSecondCycle && x.Mode == ExercisesMods.WeightCount)
                .Should().BeFalse();
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == secondDayNameInSecondCycle && x.Name == secondExerciseNameOnSecondDayInSecondCycle && x.Mode == ExercisesMods.FreeResult)
                .Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.AddExercises);

            userContext.DataManager.TempExercises.Should().HaveCount(2);
            DTOExercise? firstAddedExrecise = userContext.DataManager.TempExercises.FirstOrDefault();
            firstAddedExrecise.Should().NotBeNull();
            firstAddedExrecise.Name.Should().Be(firstExerciseNameOnSecondDayInSecondCycle);
            firstAddedExrecise.Day.Should().BeNull();
            firstAddedExrecise.Mode.Should().Be(ExercisesMods.WeightCount);

            DTOExercise? secondAddedExrecise = userContext.DataManager.TempExercises.Skip(1).FirstOrDefault();
            secondAddedExrecise.Should().NotBeNull();
            secondAddedExrecise.Name.Should().Be(secondExerciseNameOnSecondDayInSecondCycle);
            secondAddedExrecise.Day.Should().BeNull();
            secondAddedExrecise.Mode.Should().Be(ExercisesMods.FreeResult);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнение(я) зафиксировано(ы)!
======================

Введите след. упражнение(я) для дня {secondDayNameInSecondCycle.AddBoldAndQuotes()} либо нажмите ""Сохранить"" для сохранения зафиксированных упражнений
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SaveExercises, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct18(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            userContext.DataManager.TempExercises.Should().BeNull();

            entityContext.Exercises.Should().HaveCount(8);
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == secondDayNameInSecondCycle && x.Name == firstExerciseNameOnSecondDayInSecondCycle && x.Mode == ExercisesMods.WeightCount)
                .Should().BeTrue();
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Name == secondDayNameInSecondCycle && x.Name == secondExerciseNameOnSecondDayInSecondCycle && x.Mode == ExercisesMods.FreeResult)
                .Should().BeTrue();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($@"Упражнения сохранены!
======================

Выберите интересующие настройки для упражнений из дня {secondDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingExercises, ButtonsSet.SettingDays));
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
            updateHandlerResult.InformationSet.Message.Should().Be($@"Выберите интересующий раздел");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }
    }
}