using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.BusinessLogic.Repositories
{
    public class AdminWrapperTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly EntityContext entityContext;

        private readonly Mock<ICustomLoggerFactory> loggerFactoryMock;

        private readonly Mock<ILogger> loggerMock;

        private readonly AdminWrapper adminWrapper;

        public AdminWrapperTests()
        {
            builder = new EntityContextBuilder();

            entityContext = builder.Create()
                                   .WithUserInformation()
                                   .WithCycle()
                                   .Build();

            ConfigurationData configurationData = new ConfigurationData()
            {
                Bot = new BotSettings()
                {
                    WhiteListIsEnable = false,
                    OwnersChatIDs = ["1", "2"],
                    Token = "123",
                },
                DB = new DbSettings()
                {
                    Database = "Database",
                }
            };

            loggerFactoryMock = new();

            loggerMock = new();

            loggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            adminWrapper = new AdminWrapper(entityContext, configurationData, loggerFactoryMock.Object);
        }

        [Fact]
        public void GetSafeConfigurationData_ShouldReturnNotEmptyStr()
        {
            // Act
            string safeConfigutationData = adminWrapper.GetSafeConfigurationData();

            // Assert
            safeConfigutationData.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ChangeWhiteListMode_ShouldChangeWhiteListModeAndWriteWarningLog()
        {
            // Arrange
            bool currentWhiteListMode = adminWrapper.WhiteListIsEnable;

            // Act
            adminWrapper.ChangeWhiteListMode();

            // Assert
            adminWrapper.WhiteListIsEnable.Should().Be(!currentWhiteListMode);

            loggerMock.Verify(x => x.Log(
                                LogLevel.Warning,
                                It.IsAny<EventId>(),
                                It.IsAny<It.IsAnyType>(),
                                It.IsAny<Exception>(),
                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                Times.Once);
        }

        [Fact]
        public async Task ChangeBlackListByUser_ShouldChangeBlackListByUserAndWriteWarningLog()
        {
            // Arrange
            UserInformation userInformation = await entityContext.UsersInformation.FirstAsync();
            bool currentBlackListUserMode = userInformation.BlackList;

            // Act
            await adminWrapper.ChangeBlackListByUser(userInformation);

            // Assert
            userInformation.BlackList.Should().Be(!currentBlackListUserMode);

            loggerMock.Verify(x => x.Log(
                                LogLevel.Warning,
                                It.IsAny<EventId>(),
                                It.IsAny<It.IsAnyType>(),
                                It.IsAny<Exception>(),
                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                Times.Once);
        }

        [Fact]
        public async Task ChangeWhiteListByUser_ShouldChangeWhiteListByUserAndWriteWarningLog()
        {
            // Arrange
            UserInformation userInformation = await entityContext.UsersInformation.FirstAsync();
            bool currentWhiteListUserMode = userInformation.WhiteList;

            // Act
            await adminWrapper.ChangeWhiteListByUser(userInformation);

            // Assert
            userInformation.WhiteList.Should().Be(!currentWhiteListUserMode);

            loggerMock.Verify(x => x.Log(
                                LogLevel.Warning,
                                It.IsAny<EventId>(),
                                It.IsAny<It.IsAnyType>(),
                                It.IsAny<Exception>(),
                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                Times.Once);
        }

        [Fact]
        public async Task DeleteAccount_ShouldDeleteAccountAndWriteWarningLog()
        {
            // Arrange
            UserInformation userInformation = await entityContext.UsersInformation.FirstAsync();

            // Act
            await adminWrapper.DeleteAccount(userInformation);

            // Assert
            entityContext.UsersInformation.Should().BeEmpty();

            loggerMock.Verify(x => x.Log(
                                LogLevel.Warning,
                                It.IsAny<EventId>(),
                                It.IsAny<It.IsAnyType>(),
                                It.IsAny<Exception>(),
                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                Times.Once);
        }

        [Fact]
        public async Task TryCreateNewUserInformation_WithUserWithWhiteListModeIsFalse_ShouldCreateNewUserInformation()
        {
            // Arrange
            Telegram.Bot.Types.User newUser = new Telegram.Bot.Types.User()
            {
                Id = 1,
                FirstName = "TestFirtsName",
                Username = "TestUserName",
            };

            // Act 
            UserInformation? userInformation = await adminWrapper.TryCreateNewUserInformation(newUser);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.FirstName.Should().Be(newUser.FirstName);
            userInformation.Username.Should().Be($"@{newUser.Username}");

            loggerMock.Verify(x => x.Log(
                                LogLevel.Warning,
                                It.IsAny<EventId>(),
                                It.IsAny<It.IsAnyType>(),
                                It.IsAny<Exception>(),
                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                Times.Once);
        }

        [Fact]
        public async Task TryCreateNewUserInformation_WithUserWithWhiteListModeIsTrue_ShouldNotCreateNewUserInformation()
        {
            // Arrange
            Telegram.Bot.Types.User newUser = new Telegram.Bot.Types.User()
            {
                Id = 1,
                FirstName = "TestFirtsName",
                Username = "TestUserName",
            };

            adminWrapper.ChangeWhiteListMode();

            // Act 
            UserInformation? userInformation = await adminWrapper.TryCreateNewUserInformation(newUser);

            // Assert
            userInformation.Should().BeNull();

            loggerMock.Verify(x => x.Log(
                                LogLevel.Warning,
                                It.IsAny<EventId>(),
                                It.IsAny<It.IsAnyType>(),
                                It.IsAny<Exception>(),
                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                Times.Exactly(2));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task UserIsOwner_ShouldReturnUserIsOwnerOrNot(bool userWasContainsInOwners)
        {
            // Arrange
            bool userIsOwner = false;

            UserInformation userInformation = await entityContext.UsersInformation.FirstAsync();

            if (userWasContainsInOwners)
            {
                // Act
                userIsOwner = adminWrapper.UserIsOwner(userInformation.UserId);
                // Assert
                userIsOwner.Should().BeTrue();
            }
            else
            {
                userInformation.UserId = 3;
                // Act
                userIsOwner = adminWrapper.UserIsOwner(userInformation.UserId);
                // Assert
                userIsOwner.Should().BeFalse();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task UserHasAccess_ShouldReturnUserHasAccessOrNot(int testCase)
        {
            // Arrange
            bool userHasAccess = false;

            UserInformation userInformation = await entityContext.UsersInformation.FirstAsync();

            switch (testCase)
            {
                case 0:
                    userInformation.UserId = 3;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 1:
                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 2:
                    userInformation.UserId = 3;
                    userInformation.BlackList = true;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();

                    break;

                case 3:
                    userInformation.UserId = 3;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 4:
                    userInformation.UserId = 3;
                    adminWrapper.ChangeWhiteListMode();

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();
                    break;

                case 5:
                    userInformation.UserId = 3;
                    adminWrapper.ChangeWhiteListMode();
                    userInformation.WhiteList = true;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(testCase)}:{testCase}");
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task UserHasAccess_WithDTOUserInformation_ShouldReturnUserHasAccessOrNot(int testCase)
        {
            // Arrange
            bool userHasAccess = false;

            UserInformation userInformation = await entityContext.UsersInformation.FirstAsync();

            DTOUserInformation DTOuserInformation = userInformation.ToDTOUserInformation();

            switch (testCase)
            {
                case 0:
                    DTOuserInformation.UserId = 3;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 1:
                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 2:
                    DTOuserInformation.UserId = 3;
                    DTOuserInformation.BlackList = true;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();

                    break;

                case 3:
                    DTOuserInformation.UserId = 3;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 4:
                    DTOuserInformation.UserId = 3;
                    adminWrapper.ChangeWhiteListMode();

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();
                    break;

                case 5:
                    DTOuserInformation.UserId = 3;
                    adminWrapper.ChangeWhiteListMode();
                    DTOuserInformation.WhiteList = true;

                    // Act
                    userHasAccess = adminWrapper.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(testCase)}:{testCase}");
            }
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}