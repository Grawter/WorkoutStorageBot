using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.UnitTests.Helpers;

namespace WorkoutStorageBot.IntegrationTests.Core.Repositories
{
    public class RepositoriesStoreTests : IDisposable
    {
        private readonly EntityContextBuilder builder;

        private readonly EntityContext entityContext;

        public RepositoriesStoreTests()
        {
            builder = new EntityContextBuilder();
            entityContext = builder.Create().Build();
        }

        [Fact]
        public void TryAddRepository_ShouldAddRepositoryAndReturnHim()
        {
            // Arrange
            RepositoriesStore repositoriesStore = new RepositoriesStore(entityContext);

            // Act
            UserInformationRepository? repository = repositoriesStore.TryAddRepository<UserInformationRepository>();

            // Assert
            repository.Should().NotBeNull();
        }

        [Fact]
        public void InitRepository_ShouldInitRepositoryAndReturnHim()
        {
            // Arrange
            RepositoriesStore repositoriesStore = new RepositoriesStore(entityContext);
            
            // Act
            LogsRepository? repository = repositoriesStore.InitRepository(x => new LogsRepository(x));

            // Assert
            repository.Should().NotBeNull();
        }

        [Fact]
        public void InitRepository_ShouldNotInitRepositoryAndReturnHim()
        {
            // Arrange
            RepositoriesStore repositoriesStore = new RepositoriesStore(entityContext);
            repositoriesStore.TryAddRepository<UserInformationRepository>();

            // Act
            UserInformationRepository? repository = repositoriesStore.InitRepository(x => new UserInformationRepository(x));

            // Assert
            repository.Should().NotBeNull();
        }

        [Fact]
        public void GetRequiredRepository_ShouldGetExistingRepository()
        {
            // Arrange
            RepositoriesStore repositoriesStore = new RepositoriesStore(entityContext);
            repositoriesStore.TryAddRepository<UserInformationRepository>();

            // Act
            UserInformationRepository? repository = repositoriesStore.GetRequiredRepository<UserInformationRepository>();

            // Assert
            repository.Should().NotBeNull();
        }

        [Fact]
        public void GetRepository_ShouldGetExistingRepository()
        {
            // Arrange
            RepositoriesStore repositoriesStore = new RepositoriesStore(entityContext);
            repositoriesStore.TryAddRepository<UserInformationRepository>();

            // Act
            UserInformationRepository? repository = repositoriesStore.GetRepository<UserInformationRepository>();

            // Assert
            repository.Should().NotBeNull();
        }

        public void Dispose()
        {
            builder.Dispose();
        }
    }
}