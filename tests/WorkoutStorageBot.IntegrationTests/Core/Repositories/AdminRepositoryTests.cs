using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Logging;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.Core.Repositories
{
    public class AdminRepositoryTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly Mock<ILogger> loggerMock;

        private readonly AdminRepository adminRepository;

        public AdminRepositoryTests() 
        {
            builder = new EntityContextBuilder();

            Mock<ICustomLoggerFactory> loggerFactoryMock = new();

            loggerMock = new();

            loggerFactoryMock.Setup(x => x.CreateLogger<It.IsAnyType>()).Returns(loggerMock.Object);

            EntityContext entityContext = builder.Create()
                                                 .WithUserInformation()
                                                 .WithCycle()
                                                 .Build();

            CoreTools coreTools = new CoreTools()
            {
                Db = entityContext,

                ConfigurationData = new Application.Configuration.ConfigurationData()
                {
                    Bot = new Application.Configuration.BotSettings()
                    { 
                        WhiteListIsEnable = false,
                        OwnersChatIDs = ["1", "2"],
                        Token = "123",
                    },
                    DB = new Application.Configuration.DbSettings()
                    { 
                        Database = "Database",
                    }
                },
                LoggerFactory = loggerFactoryMock.Object,
            };

            adminRepository = new AdminRepository(coreTools, null);
        }

        [Fact]
        public void GetSafeConfigurationData_ShouldReturnNotEmptyStr()
        {
            // Act
            string safeConfigutationData = adminRepository.GetSafeConfigurationData();

            // Assert
            safeConfigutationData.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ChangeWhiteListMode_ShouldChangeWhiteListModeAndWriteWarningLog()
        {
            // Arrange
            bool currentWhiteListMode = adminRepository.WhiteListIsEnable;

            // Act
            adminRepository.ChangeWhiteListMode();

            // Assert
            adminRepository.WhiteListIsEnable.Should().Be(!currentWhiteListMode);

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
            UserInformation userInformation = await adminRepository.CoreTools.Db.UsersInformation.FirstAsync();
            bool currentBlackListUserMode = userInformation.BlackList;

            // Act
            await adminRepository.ChangeBlackListByUser(userInformation);

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
            UserInformation userInformation = await adminRepository.CoreTools.Db.UsersInformation.FirstAsync();
            bool currentWhiteListUserMode = userInformation.WhiteList;

            // Act
            await adminRepository.ChangeWhiteListByUser(userInformation);

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
            UserInformation userInformation = await adminRepository.CoreTools.Db.UsersInformation.FirstAsync();

            // Act
            await adminRepository.DeleteAccount(userInformation);

            // Assert
            adminRepository.CoreTools.Db.UsersInformation.Should().BeEmpty();

            loggerMock.Verify(x => x.Log(
                                LogLevel.Warning,
                                It.IsAny<EventId>(),
                                It.IsAny<It.IsAnyType>(),
                                It.IsAny<Exception>(),
                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                Times.Once);
        }

        [Fact]
        public async Task GetUserInformationWithoutTracking_WithUserID_ShouldReturnUserInformation()
        {
            // Arrange
            long userID = 1;

            // Act
            UserInformation? userInformation = await adminRepository.GetUserInformationWithoutTracking(userID);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.UserId.Should().Be(userID);
        }

        [Fact]
        public async Task GetUserInformationWithoutTracking_WithUserName_ShouldReturnUserInformation()
        {
            // Arrange
            long userID = 1;

            // Act 
            UserInformation? userInformation = await adminRepository.GetUserInformationWithoutTracking(userID);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.UserId.Should().Be(userID);
        }

        [Fact]
        public async Task GetRequiredUserInformation_WithUserID_ShouldReturnUserInformation()
        {
            // Arrange
            long userID = 1;

            // Act 
            UserInformation userInformation = await adminRepository.GetRequiredUserInformation(userID);

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
            UserInformation? userInformation = await adminRepository.GetUserInformation(userID);

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
            UserInformation userInformation = await adminRepository.GetRequiredUserInformation(userName);

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
            UserInformation? userInformation = await adminRepository.GetUserInformation(userName);

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
            UserInformation? userInformation = await adminRepository.GetFullUserInformationWithoutTracking(userID);

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
            UserInformation? userInformation = await adminRepository.GetFullUserInformationWithoutTracking(userName);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.Username.Should().Be(userName);
            userInformation.Cycles.Should().NotBeNullOrEmpty();
            userInformation.Cycles.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task TryCreateNewUserInformation_WithUser_ShouldCreateNewUserInformation()
        {
            // Arrange
            Telegram.Bot.Types.User newUser = new Telegram.Bot.Types.User()
            {
                Id = 1,
                FirstName = "TestFirtsName",
                Username = "TestUserName",
            };

            // Act 
            UserInformation? userInformation = await adminRepository.TryCreateNewUserInformation(newUser);

            // Assert
            userInformation.Should().NotBeNull();
            userInformation.FirstName.Should().Be(newUser.FirstName);
            userInformation.Username.Should().Be($"@{newUser.Username}");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void UserIsOwner_ShouldReturnUserIsOwnerOrNot(bool userWasContainsInOwners)
        {
            // Arrange
            bool userIsOwner = false;

            UserInformation userInformation = adminRepository.CoreTools.Db.UsersInformation.First();

            if (userWasContainsInOwners)
            {
                // Act
                userIsOwner = adminRepository.UserIsOwner(userInformation.UserId);
                // Assert
                userIsOwner.Should().BeTrue();
            }
            else
            {
                userInformation.UserId = 3;
                // Act
                userIsOwner = adminRepository.UserIsOwner(userInformation.UserId);
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
        public void UserHasAccess_ShouldReturnUserHasAccessOrNot(int testCase)
        {
            // Arrange
            bool userHasAccess = false;

            UserInformation userInformation = adminRepository.CoreTools.Db.UsersInformation.First();

            switch (testCase)
            {
                case 0:
                    userInformation.UserId = 3;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 1:
                    // Act
                    userHasAccess = adminRepository.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 2:
                    userInformation.UserId = 3;
                    userInformation.BlackList = true;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();

                    break;

                case 3:
                    userInformation.UserId = 3;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 4:
                    userInformation.UserId = 3;
                    adminRepository.ChangeWhiteListMode();

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(userInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();
                    break;

                case 5:
                    userInformation.UserId = 3;
                    adminRepository.ChangeWhiteListMode();
                    userInformation.WhiteList = true;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(userInformation);

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
        public void UserHasAccess_WithDTOUserInformation_ShouldReturnUserHasAccessOrNot(int testCase)
        {
            // Arrange
            bool userHasAccess = false;

            DTOUserInformation DTOuserInformation = adminRepository.CoreTools.Db.UsersInformation.First().ToDTOUserInformation();

            switch (testCase)
            {
                case 0:
                    DTOuserInformation.UserId = 3;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 1:
                    // Act
                    userHasAccess = adminRepository.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 2:
                    DTOuserInformation.UserId = 3;
                    DTOuserInformation.BlackList = true;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();

                    break;

                case 3:
                    DTOuserInformation.UserId = 3;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeTrue();
                    break;

                case 4:
                    DTOuserInformation.UserId = 3;
                    adminRepository.ChangeWhiteListMode();

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(DTOuserInformation);

                    // Assert
                    userHasAccess.Should().BeFalse();
                    break;

                case 5:
                    DTOuserInformation.UserId = 3;
                    adminRepository.ChangeWhiteListMode();
                    DTOuserInformation.WhiteList = true;

                    // Act
                    userHasAccess = adminRepository.UserHasAccess(DTOuserInformation);

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