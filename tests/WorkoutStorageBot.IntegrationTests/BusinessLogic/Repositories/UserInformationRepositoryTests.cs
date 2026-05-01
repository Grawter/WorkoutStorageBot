using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.BusinessLogic.Repositories
{
    public class UserInformationRepositoryTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly Mock<ILogger> loggerMock;

        private readonly UserInformationRepository userInformationRepository;

        public UserInformationRepositoryTests() 
        {
            builder = new EntityContextBuilder();

            Mock<ICustomLoggerFactory> loggerFactoryMock = new();

            loggerMock = new();

            loggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            EntityContext entityContext = builder.Create()
                                                 .WithUserInformation()
                                                 .WithCycle()
                                                 .Build();

            userInformationRepository = new UserInformationRepository(entityContext);
        }

        [Fact]
        public async Task GetUserInformationWithoutTracking_WithUserID_ShouldReturnUserInformation()
        {
            // Arrange
            long userID = 1;

            // Act
            UserInformation? userInformation = await userInformationRepository.GetUserInformationWithoutTracking(userID);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.UserId.Should().Be(userID);
        }

        [Fact]
        public async Task GetUserInformationWithoutTracking_WithUserName_ShouldReturnUserInformation()
        {
            // Arrange
            string userName = "@TestUsername";

            // Act 
            UserInformation? userInformation = await userInformationRepository.GetUserInformationWithoutTracking(userName);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.Username.Should().Be(userName);
        }

        [Fact]
        public async Task GetRequiredUserInformation_WithUserID_ShouldReturnUserInformation()
        {
            // Arrange
            long userID = 1;

            // Act 
            UserInformation userInformation = await userInformationRepository.GetRequiredUserInformation(userID);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.UserId.Should().Be(userID);
        }

        [Fact]
        public async Task GetUserInformation_WithUserID_ShouldReturnUserInformation()
        {
            // Arrange
            long userID = 1;

            // Act 
            UserInformation? userInformation = await userInformationRepository.GetUserInformation(userID);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.UserId.Should().Be(userID);
        }

        [Fact]
        public async Task GetRequiredUserInformation_WithUserName_ShouldReturnUserInformation()
        {
            // Arrange
            string userName = "@TestUsername";

            // Act
            UserInformation userInformation = await userInformationRepository.GetRequiredUserInformation(userName);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.Username.Should().Be(userName);
        }

        [Fact]
        public async Task GetUserInformation_WithUserName_ShouldReturnUserInformation()
        {
            // Arrange
            string userName = "@TestUsername";

            // Act
            UserInformation? userInformation = await userInformationRepository.GetUserInformation(userName);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.Username.Should().Be(userName);
        }

        [Fact]
        public async Task GetFullUserInformationWithoutTracking_WithUserID_ShouldReturnUserInformation()
        {
            // Arrange
            long userID = 1;

            // Act 
            UserInformation? userInformation = await userInformationRepository.GetFullUserInformationWithoutTracking(userID);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.UserId.Should().Be(userID);
            userInformation.Cycles.Should().NotBeNullOrEmpty();
            userInformation.Cycles.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetFullUserInformationWithoutTracking_WithUserName_ShouldReturnUserInformation()
        {
            // Arrange
            string userName = "@TestUsername";

            // Act 
            UserInformation? userInformation = await userInformationRepository.GetFullUserInformationWithoutTracking(userName);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.Username.Should().Be(userName);
            userInformation.Cycles.Should().NotBeNullOrEmpty();
            userInformation.Cycles.Count.Should().BeGreaterThan(0);
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}