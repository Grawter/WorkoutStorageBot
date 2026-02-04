using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Exceptions
{
    public class UserInformationExtensionsTests
    {
        [Fact]
        public void IsAdmin_UserContextWithAdminRole_ShouldBeAdmin()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();

            DTOUserInformation dTOUserInformation = DTOModelBuilder.DTOTestUserInformation;

            UserContext userContext = new UserContext(dTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.Admin, false);
            
            // Act
            bool isAdmin = userContext.IsAdmin();

            // Assert
            isAdmin.Should().BeTrue();
        }

        [Fact]
        public void IsAdmin_UserContextWithoutAdminRole_ShouldNotBeAdmin()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();

            DTOUserInformation dTOUserInformation = DTOModelBuilder.DTOTestUserInformation;

            UserContext userContext = new UserContext(dTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);
            // Act
            bool isAdmin = userContext.IsAdmin();

            // Assert
            isAdmin.Should().BeFalse();
        }
    }
}