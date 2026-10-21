using FluentAssertions;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

namespace WorkoutStorageBot.E2ETests.Scenarios.Scenario1
{
    internal class Scenario1BusinessAct6AssertsTestsHelper : BaseAssertsBusinessActsAssertsHelper
    {
        private readonly string firstCycleName;
        private readonly string secondCycleName;

        private readonly string firstDayNameInSecondCycle;
        private readonly string secondDayNameInSecondCycle;


        internal Scenario1BusinessAct6AssertsTestsHelper(IContextKeeper contextKeeper,
                                                         EntityContext entityContext,
                                                         string userName,
                                                         long userID,
                                                         string firstName,
                                                         int userInformationID,

                                                         string firstCycleName,
                                                         string secondCycleName,

                                                         string firstDayNameInSecondCycle,
                                                         string secondDayNameInSecondCycle)
            : base(contextKeeper,
                    entityContext,
                    userName,
                    userID,
                    firstName,
                    userInformationID)
        {
            this.firstCycleName = firstCycleName;
            this.secondCycleName = secondCycleName;

            this.firstDayNameInSecondCycle = firstDayNameInSecondCycle;
            this.secondDayNameInSecondCycle = secondDayNameInSecondCycle;
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
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующие настройки для дней из цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDays, ButtonsSet.SettingCycle));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct6(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующий день из цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct7(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующую настройку для дня {secondDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDay, ButtonsSet.DaysList));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct8(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Should().HaveCount(4);
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == secondDayNameInSecondCycle).Should().BeTrue();
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == "22ToDelete").Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.ChangeNameDay);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Введите новоё название для дня {secondDayNameInSecondCycle.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.SettingDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct9(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Should().HaveCount(4);
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == secondDayNameInSecondCycle).Should().BeFalse();
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == "22ToDelete").Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Название дня сохранено!
======================

Выберите интересующую настройку для указанного дня
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDay, ButtonsSet.DaysList));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }  

        internal async Task AssertAct10(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x =>
                                        x.Cycle != null
                                        && x.Cycle.Name == secondCycleName
                                        && x.Name == "22ToDelete"
                                        && x.IsArchive == true).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"День {"22ToDelete".AddBoldAndQuotes()} был добавлен в архив
======================

Выберите интересующий день из цикла {secondCycleName.AddBoldAndQuotes()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct11(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующие настройки для дней из цикла {secondCycleName.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDays, ButtonsSet.SettingCycle));
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
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Выберите интересующий раздел");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
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
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующий архив для разархивирования");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveList, ButtonsSet.Settings));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct14(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x =>
                                        x.Cycle != null
                                        && x.Cycle.Name == secondCycleName
                                        && x.Name == "22ToDelete"
                                        && x.IsArchive == false).Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите архивный день для разархивирования");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct15(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x =>
                                        x.Cycle != null
                                        && x.Cycle.Name == secondCycleName
                                        && x.Name == "22ToDelete"
                                        && x.IsArchive == false).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"{"22ToDelete".AddBoldAndQuotes()} разархивирован!");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ArchiveList, ButtonsSet.Settings));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct16(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующую настройку для дня {"22ToDelete".AddBoldAndQuotes()} ({secondCycleName.AddBold()})");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDay, ButtonsSet.DaysList));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct17(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Should().HaveCount(4);
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == "22ToDelete").Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Вы уверены?
======================

<b>Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне</b>
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.SettingDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().HaveCount(2);

            KeyValuePair<string, string> firstParam = updateHandlerResult.InformationSet.AdditionalParameters.First();
            firstParam.Key.Should().Be("DomainType");
            firstParam.Value.Should().Be("Day");

            KeyValuePair<string, string> secondParam = updateHandlerResult.InformationSet.AdditionalParameters.Skip(1).First();
            secondParam.Key.Should().Be("Name");
            secondParam.Value.Should().Be("день");

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct18(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Should().HaveCount(3);
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName && x.Name == "22ToDelete").Should().BeFalse();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"День {"22ToDelete".AddBoldAndQuotes()} удалён!
======================

Выберите интересующий день из цикла {secondCycleName.AddBoldAndQuotes()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.DaysList, ButtonsSet.SettingDays));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct19(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующую настройку для дня {firstDayNameInSecondCycle.AddBoldAndQuotes()} ({secondCycleName.AddBold()})");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.SettingDay, ButtonsSet.DaysList));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct20(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName).Should().BeTrue();
            entityContext.Days.Where(x => x.Cycle != null && x.Cycle.Name == firstCycleName).Should().HaveCount(2);

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Выберите цикл, в который хотите перенести день {firstDayNameInSecondCycle.AddBoldAndQuotes()}");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct21(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.Name == secondCycleName).Should().BeFalse();
            entityContext.Days.Where(x => x.Cycle != null && x.Cycle.Name == firstCycleName).Should().HaveCount(3);

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"День {firstDayNameInSecondCycle.AddBoldAndQuotes()}, перенесён в цикл {firstCycleName.AddBoldAndQuotes()}
======================

Выберите интересующий цикл
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.CycleList, ButtonsSet.SettingCycles));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct22(List<HandlerResult> handlerResults)
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

        internal async Task AssertAct23(List<HandlerResult> handlerResults)
        {
            await AssertBaseUserInformation();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.NoMatter);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Выберите интересующий раздел");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.Main, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }
    }
}