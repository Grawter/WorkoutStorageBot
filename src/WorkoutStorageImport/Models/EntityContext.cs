using Microsoft.EntityFrameworkCore;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Entities.Core.Logging;
using WorkoutStorageModels.Entities.Import;

namespace WorkoutStorageImport.Models
{
    public class EntityContext : DbContext
    {
        public DbSet<UserInformation> UsersInformation => Set<UserInformation>();
        public DbSet<Cycle> Cycles => Set<Cycle>();
        public DbSet<Day> Days => Set<Day>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<ResultExercise> ResultsExercises => Set<ResultExercise>();
        public DbSet<Log> Logs => Set<Log>();
        public DbSet<ImportInfo> ImportInfo => Set<ImportInfo>();

        public EntityContext(DbContextOptions<EntityContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}