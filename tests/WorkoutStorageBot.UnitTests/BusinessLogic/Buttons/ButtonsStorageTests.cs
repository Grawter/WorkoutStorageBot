using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Buttons.Storage;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Buttons
{
    public class ButtonsStorageTests
    {
        [Fact]
        public void GetButtonsFactory_WithButtonsSetAndUserContext_ShouldReturnExistingButtonsFactory()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, Roles.User, false);

            // Act
            ButtonsFactory someBF = ButtonsStorage.GetButtonsFactory(ButtonsSet.None, userContext);

            // Assert
            someBF.Should().NotBeNull();
        }
    }
}