using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Repositories.Abstraction;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Core.Repositories.Store
{
    internal class RepositoriesStore
    {
        private Lazy<List<CoreRepository>> lazyRepositories;

        private List<CoreRepository> Repositories => lazyRepositories.Value;

        private readonly EntityContext db;

        internal RepositoriesStore(EntityContext db)
        {
            this.db = db;

            lazyRepositories = new Lazy<List<CoreRepository>>();
        }

        internal T GetRequiredRepository<T>() where T : CoreRepository
        {
            T? repository = GetRepository<T>();

            if (repository == null)
                throw new InvalidOperationException($"Репозиторий {typeof(T).Name} не найден");

            return repository;
        }

        internal T? GetRepository<T>() where T : CoreRepository
        {
           T? repository = Repositories.OfType<T>().FirstOrDefault();

            if (repository == null)
            {
                repository = TryAddRepository<T>(typeof(T).Name);
            }

            return repository;
        }
            
        internal T? TryAddRepository<T>(string repositoryName) where T : CoreRepository
        {
            CoreRepository? coreRepository = null;

            switch (repositoryName)
            {
                case nameof(UserInformationRepository):
                    coreRepository = new UserInformationRepository(db);
                    Repositories.Add(coreRepository);

                    break;
                case nameof(LogsRepository):
                    coreRepository = new LogsRepository(db);
                    Repositories.Add(coreRepository);
                    break;
            }

            return (T?)coreRepository;
        }

        internal T InitRepository<T>(Func<EntityContext, T> init) where T : CoreRepository
        {
            T? repository = Repositories.OfType<T>().FirstOrDefault();

            if (repository == null)
            {
                repository = init(db);
                Repositories.Add(repository);
            }

            return repository;
        }
    }
}