using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Model.Entities.BusinessLogic;
using WorkoutStorageBot.Model.Entities.Import;
using WorkoutStorageBot.Model.Entities.Logging;

namespace WorkoutStorageBot.Model.AppContext
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
        { }
    }
}