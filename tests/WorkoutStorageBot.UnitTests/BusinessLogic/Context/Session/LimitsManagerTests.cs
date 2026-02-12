using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Context.Session
{
    public class LimitsManagerTests
    {
        [Fact]
        public void ChangeLimitsMode_WithOffLimitsManager_ShouldOnLimitsManager()
        {
            // Arrange 
            LimitsManager LimitsManager = new LimitsManager(false);

            ILogger logger = new Mock<ILogger>().Object;

            // Act
            LimitsManager.ChangeLimitsMode(logger);

            // Assert
            LimitsManager.IsEnableLimit.Should().BeTrue();
        }

        [Fact]
        public void ChangeLimitsMode_WithOnLimitsManager_ShouldOffLimitsManager()
        {
            // Arrange 
            LimitsManager LimitsManager = new LimitsManager(true);

            ILogger logger = new Mock<ILogger>().Object;

            // Act
            LimitsManager.ChangeLimitsMode(logger);

            // Assert
            LimitsManager.IsEnableLimit.Should().BeFalse();
        }

        [Fact]
        public void AddOrUpdateTimeLimit_WithOffLimitsManager_ShouldNotAddOrUpdateTimeLimit()
        {
            // Arrange 
            LimitsManager LimitsManager = new LimitsManager(false);
            DateTime endLimitDate = DateTime.Now.AddHours(1);

            // Act
            LimitsManager.AddOrUpdateTimeLimit("testLimit", endLimitDate);

            // Assert
            LimitsManager.IsEnableLimit.Should().BeFalse();
            LimitsManager.HasBlockByTimeLimit("testLimit", out DateTime findedEndLimitDate).Should().BeFalse();
            endLimitDate.Should().NotBe(findedEndLimitDate);
        }

        [Fact]
        public void AddOrUpdateTimeLimit_WithOnLimitsManager_ShouldAddOrUpdateTimeLimit()
        {
            // Arrange 
            LimitsManager LimitsManager = new LimitsManager(true);
            DateTime endLimitDate = DateTime.Now.AddHours(1);

            // Act
            LimitsManager.AddOrUpdateTimeLimit("testLimit", endLimitDate);

            // Assert
            LimitsManager.IsEnableLimit.Should().BeTrue();
            LimitsManager.HasBlockByTimeLimit("testLimit", out DateTime findedEndLimitDate).Should().BeTrue();
            endLimitDate.Should().Be(findedEndLimitDate);
        }

        [Fact]
        public void AddOrUpdateTimeLimit_WithOnLimitsManagerLater_ShouldAddOrUpdateTimeLimit()
        {
            // Arrange 
            LimitsManager LimitsManager = new LimitsManager(false);
            DateTime endLimitDate = DateTime.Now.AddHours(1);

            ILogger logger = new Mock<ILogger>().Object;

            LimitsManager.ChangeLimitsMode(logger);

            // Act
            LimitsManager.AddOrUpdateTimeLimit("testLimit", endLimitDate);

            // Assert
            LimitsManager.IsEnableLimit.Should().BeTrue();
            LimitsManager.HasBlockByTimeLimit("testLimit", out DateTime findedEndLimitDate).Should().BeTrue();
            endLimitDate.Should().Be(findedEndLimitDate);
        }

        [Fact]
        public void AddOrUpdateTimeLimit_UpdateExistingTimeLimit_ShouldUpdateExistingTimeLimit()
        {
            // Arrange 
            LimitsManager LimitsManager = new LimitsManager(true);
            DateTime oldEndLimitDate = DateTime.Now.AddHours(1);
            LimitsManager.AddOrUpdateTimeLimit("testLimit", oldEndLimitDate);

            DateTime newEndLimitDate = DateTime.Now.AddHours(2);

            // Act
            LimitsManager.AddOrUpdateTimeLimit("testLimit", newEndLimitDate);

            // Assert
            LimitsManager.IsEnableLimit.Should().BeTrue();
            LimitsManager.HasBlockByTimeLimit("testLimit", out DateTime findedEndLimitDate).Should().BeTrue();
            findedEndLimitDate.Should().Be(newEndLimitDate);
            findedEndLimitDate.Should().NotBe(oldEndLimitDate);
        }
    }
}