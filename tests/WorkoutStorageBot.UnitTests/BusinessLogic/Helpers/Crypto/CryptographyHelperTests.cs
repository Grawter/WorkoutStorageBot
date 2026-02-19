using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Helpers.Crypto;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Crypto
{
    public class CryptographyHelperTests
    {
        [Fact]
        public void CreateRandomCallBackQueryId_ShouldCreateRandomString()
        {
            // Arrange && Act
            string randomString1 = CryptographyHelper.CreateRandomCallBackQueryId(6);
            string randomString2 = CryptographyHelper.CreateRandomCallBackQueryId(6);

            // Assert
            randomString1.Should().HaveLength(8);
            randomString2.Should().HaveLength(8);
            randomString1.Should().NotBe(randomString2);
        }
    }
}