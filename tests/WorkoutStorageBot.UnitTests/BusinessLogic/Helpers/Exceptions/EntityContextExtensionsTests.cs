using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.es
{
    public class EntityContextExtensionsTests
    {
        [Fact]
        public async Task AddEntity_ShouldSaveCycleWithNameAndGenerateId()
        {
            using EntityContext context = CreateInMemoryDbContextWithSeedData();

            UserInformation testUser = context.UsersInformation.First();

            IDTOByEntity dtoCycle = new DTOCycle() { UserInformationId = testUser.Id, Name = "TestCycle" };

            // Act
            await context.AddEntity(dtoCycle, true);

            // Assert
            Cycle? savedCycle = context.Cycles.Find(dtoCycle.Id);
            savedCycle.Should().NotBeNull();
            savedCycle.Name.Should().Be("TestCycle");
            dtoCycle.Id.Should().NotBe(0);
        }

        private static EntityContext CreateInMemoryDbContextWithSeedData()
        {
            var options = new DbContextOptionsBuilder<EntityContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;

            var context = new EntityContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            // Seed
            context.UsersInformation.Add(new UserInformation
            {
                FirstName = "TestFirstName",
                Username = "@TestUsername"
            });

            context.SaveChanges();

            return context;
        }
    }
}
