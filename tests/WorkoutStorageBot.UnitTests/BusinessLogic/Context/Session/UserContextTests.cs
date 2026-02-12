using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Context.Session
{
    public class UserContextTests
    {
        [Fact]
        public void UpdateActiveCycleForce_WithNewDTOCycle_ShouldSetNewActiveCycle()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation()
                                                                   .WithDTOCycleByDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            DTOCycle? oldCycle = userContext.ActiveCycle;

            DTOCycle newActiveCycle = new DTOCycle
            {
                Id = 15,
                Name = "Cycle 2",
                IsActive = true,
                IsArchive = true
            };

            // Act
            userContext.SetNewActiveCycleForce(newActiveCycle);

            // Assert
            userContext.ActiveCycle.Should().NotBe(oldCycle);
            userContext.ActiveCycle.Should().Be(newActiveCycle);
        }

        [Fact]
        public void GenerateNewCallBackId_ShouldGenerateNewCallBackId()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            string oldCallBackId = userContext.CallBackId;

            // Act
            userContext.GenerateNewCallBackId();

            // Assert
            userContext.CallBackId.Should().NotBe(string.Empty);
            userContext.CallBackId.Should().NotBe(oldCallBackId);
        }
    }
}