using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.E2ETests.Scenarios
{
    internal class BaseAssertsBusinessActsAssertsHelper
    {
        protected readonly IContextKeeper contextKeeper;
        protected readonly EntityContext entityContext;
        protected readonly string userName;
        protected readonly long userID;
        protected readonly string firstName;
        protected readonly int userInformationID;

        internal BaseAssertsBusinessActsAssertsHelper(IContextKeeper contextKeeper,
                                                        EntityContext entityContext,
                                                        string userName,
                                                        long userID,
                                                        string firstName,
                                                        int userInformationID)
        {
            this.contextKeeper = contextKeeper;
            this.entityContext = entityContext;

            this.userName = userName;
            this.userID = userID;
            this.firstName = firstName;
            this.userInformationID = userInformationID;
        }

        protected async Task AssertBaseUserInformation()
        {
            UserInformation? userInformation = await entityContext.UsersInformation.FindAsync(userInformationID);
            userInformation.Should().NotBeNull();

            userInformation.Username.Should().Be(userName);
            userInformation.UserId.Should().Be(userID);
            userInformation.FirstName.Should().Be(firstName);
        }

        protected async Task<UserContext> AssertBaseUserContext()
        {
            UserContext? userContext = contextKeeper.GetContext(userID);

            userContext.Should().NotBeNull();
            userContext.UserInformation.Username.Should().Be(userName);
            userContext.UserInformation.UserId.Should().Be(userID);
            userContext.UserInformation.FirstName.Should().Be(firstName);

            return userContext;
        }

        protected void AssertBasePrimatyHandlerResult(List<HandlerResult> handlerResults)
        {
            handlerResults.Should().HaveCount(2);

            PrimaryHandlerResult primaryHandlerResult = (PrimaryHandlerResult)handlerResults[0];
            primaryHandlerResult.InformationSet.Should().BeNull();
            primaryHandlerResult.IsNewContext.Should().BeFalse();
            primaryHandlerResult.IsNeedStopFurtherProcessing.Should().BeFalse();
        }

        protected void AssertBaseUpdateHandlerResult(UpdateHandlerResult updateHandlerResult)
        {
            updateHandlerResult.IsNeedSendResponse.Should().BeTrue();
            updateHandlerResult.IsNeedStopFurtherProcessing.Should().BeFalse();
        }
    }
}