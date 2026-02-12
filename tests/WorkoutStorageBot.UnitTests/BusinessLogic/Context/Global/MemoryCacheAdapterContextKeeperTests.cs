using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Context.Global
{
    public class MemoryCacheAdapterContextKeeperTests
    {
        [Fact]
        public void CountProperty_WithNewContextKeeper_ShouldBeZero()
        {
            // Arrange
            IContextKeeper context = new MemoryCacheAdapterContextKeeper();

            // Act & Assert
            context.Count.Should().Be(0);
        }

        [Fact]
        public void GetAllKeys_WithNewContextKeeper_ShouldBeEmpty()
        {
            // Arrange
            IContextKeeper context = new MemoryCacheAdapterContextKeeper();

            // Act & Assert
            context.GetAllKeys().Should().BeEmpty();
        }

        [Fact]
        public void SetContext_WithExistingContextKeeper_ShouldSaveContext()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);

            IContextKeeper context = new MemoryCacheAdapterContextKeeper();

            int userID = 123;

            // Act 
            context.SetContext(userID, userContext);

            // Assert
            context.Count.Should().Be(1);
            context.HasContext(userID).Should().BeTrue();
            context.GetAllKeys().Should().Contain(userID);
        }

        [Fact]
        public void GetContext_WithExistingContextKeeper_ShouldReturnExistingContext()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);

            IContextKeeper context = new MemoryCacheAdapterContextKeeper();

            int userID = 123;
            context.SetContext(userID, userContext);

            // Act
            UserContext? existingContext = context.GetContext(userID);

            // Assert
            existingContext.Should().NotBeNull();
            context.Count.Should().Be(1);
            context.HasContext(userID).Should().BeTrue();
            context.GetAllKeys().Should().Contain(userID);
        }

        [Fact]
        public void RemoveContext_WithExistingContextKeeper_ShouldRemoveContext()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            UserContext userContext = new UserContext(DTOUserInformation, WorkoutStorageBot.BusinessLogic.Enums.Roles.User, false);

            IContextKeeper context = new MemoryCacheAdapterContextKeeper();

            int userID = 123;
            context.SetContext(userID, userContext);

            // Act
            context.RemoveContext(userID);

            // Assert
            context.Count.Should().Be(0);
            context.HasContext(userID).Should().BeFalse();
            context.GetAllKeys().Should().NotContain(userID);
        }
    }
}