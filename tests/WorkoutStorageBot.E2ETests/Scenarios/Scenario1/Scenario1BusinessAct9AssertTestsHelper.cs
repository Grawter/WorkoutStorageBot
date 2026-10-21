using FluentAssertions;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.E2ETests.Scenarios.Scenario1
{
    internal class Scenario1BusinessAct9AssertsTestsHelper : BaseAssertsBusinessActsAssertsHelper
    {
        internal Scenario1BusinessAct9AssertsTestsHelper(IContextKeeper contextKeeper,
                                                         EntityContext entityContext,
                                                         string userName,
                                                         long userID,
                                                         string firstName,
                                                         int userInformationID)
            : base(contextKeeper,
                    entityContext,
                    userName,
                    userID,
                    firstName,
                    userInformationID)
        {
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

            entityContext.UsersInformation.Should().HaveCount(1);
            entityContext.UsersInformation.Any(x => x.UserId == userID).Should().BeTrue();
            entityContext.Cycles.Any(x => x.UserInformationId == userInformationID).Should().BeTrue();
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.UserInformationId == userInformationID).Should().BeTrue();
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Cycle != null && x.Day.Cycle.UserInformationId == userInformationID).Should().BeTrue();
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Day != null && x.Exercise.Day.Cycle != null && x.Exercise.Day.Cycle.UserInformationId == userInformationID).Should().BeTrue();

            UserContext userContext = await AssertBaseUserContext();
            userContext.Navigation.QueryFrom.Should().Be(QueryFrom.Settings);
            userContext.Navigation.MessageNavigationTarget.Should().Be(MessageNavigationTarget.Default);

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be(@$"Вы уверены?
======================

{"Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках".AddBold()}
");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.ConfirmDelete, ButtonsSet.Settings));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);

            updateHandlerResult.InformationSet.AdditionalParameters.Should().HaveCount(2);

            KeyValuePair<string, string> firstParam = updateHandlerResult.InformationSet.AdditionalParameters.First();
            firstParam.Key.Should().Be("DomainType");
            firstParam.Value.Should().Be("Account");

            KeyValuePair<string, string> secondParam = updateHandlerResult.InformationSet.AdditionalParameters.Skip(1).First();
            secondParam.Key.Should().Be("Name");
            secondParam.Value.Should().Be("аккаунт");

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct3(List<HandlerResult> handlerResults)
        {
            UserInformation? userInformation = await entityContext.UsersInformation.FindAsync(userInformationID);
            userInformation.Should().BeNull();

            entityContext.UsersInformation.Should().HaveCount(0);
            entityContext.UsersInformation.Any(x => x.UserId == userID).Should().BeFalse();
            entityContext.Cycles.Any(x => x.UserInformationId == userInformationID).Should().BeFalse();
            entityContext.Days.Any(x => x.Cycle != null && x.Cycle.UserInformationId == userInformationID).Should().BeFalse();
            entityContext.Exercises.Any(x => x.Day != null && x.Day.Cycle != null && x.Day.Cycle.UserInformationId == userInformationID).Should().BeFalse();
            entityContext.ResultsExercises.Any(x => x.Exercise != null && x.Exercise.Day != null && x.Exercise.Day.Cycle != null && x.Exercise.Day.Cycle.UserInformationId == userInformationID).Should().BeFalse();

            UserContext? userContext = contextKeeper.GetContext(userID);
            userContext.Should().BeNull();

            AssertBasePrimatyHandlerResult(handlerResults);

            UpdateHandlerResult updateHandlerResult = (UpdateHandlerResult)handlerResults[1];

            updateHandlerResult.InformationSet.Should().NotBeNull();
            updateHandlerResult.InformationSet.Message.Should().Be($"Аккаунт успешно удалён");
            updateHandlerResult.InformationSet.ButtonsSets.Should().Be((ButtonsSet.None, ButtonsSet.None));
            updateHandlerResult.InformationSet.ParseMode.Should().Be(ParseMode.Html);
            updateHandlerResult.InformationSet.AdditionalParameters.Should().BeNull();

            AssertBaseUpdateHandlerResult(updateHandlerResult);
        }

        internal async Task AssertAct4(List<HandlerResult> handlerResults)
        {
            PrimaryHandlerResult handlerResult = (PrimaryHandlerResult)handlerResults.First();

            UserInformation? userInformation = await entityContext.UsersInformation.FindAsync(2);
            userInformation.Should().NotBeNull();

            userInformation.Username.Should().Be(userName);
            userInformation.UserId.Should().Be(userID);
            userInformation.FirstName.Should().Be(firstName);

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
        }
    }
}