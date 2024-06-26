﻿#region using
using Microsoft.EntityFrameworkCore;

#endregion

namespace WorkoutStorageBot.Model
{
    internal class EntityContext : DbContext
    {
        public DbSet<UserInformation> UsersInformation => Set<UserInformation>();
        public DbSet<Cycle> Cycles => Set<Cycle>();
        public DbSet<Day> Days => Set<Day>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<ResultExercise> ResultsExercises => Set<ResultExercise>();

        public EntityContext(DbContextOptions<EntityContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}