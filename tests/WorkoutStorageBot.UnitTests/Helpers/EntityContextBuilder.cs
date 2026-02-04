using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.Helpers
{
    internal class EntityContextBuilder : IDisposable
    {
        private SqliteConnection connection;
        private DbContextOptions<EntityContext> options;

        private EntityContext context;
        private bool contextWasInit;
        private EntityContext Context => contextWasInit
            ? context
            : throw new InvalidOperationException($"{nameof(context)} not initialized");


        private UserInformation testUserInformation;
        private bool testUserInformationWasInit;
        internal UserInformation TestUserInformation => testUserInformationWasInit
            ? testUserInformation
            : throw new InvalidOperationException($"{nameof(testUserInformation)} not initialized");


        private List<UserInformation> testUserInformations;
        private bool testUserInformationsWasInit;
        internal List<UserInformation> TestUserInformations => testUserInformationsWasInit 
            ? testUserInformations
            : throw new InvalidOperationException($"{nameof(testUserInformations)} not initialized");


        private int UserInformationId => testUserInformationWasInit
                ? TestUserInformation.Id
                : TestUserInformations.First().Id;


        private Cycle testCycle;
        private bool testCycleWasInit;
        internal Cycle TestCycle => testCycleWasInit
            ? testCycle
            : throw new InvalidOperationException($"{nameof(testCycle)} not initialized");


        private List<Cycle> testCycles;
        private bool testCyclesWasInit;
        internal List<Cycle> TestCycles => testCyclesWasInit
            ? testCycles
            : throw new InvalidOperationException($"{nameof(testCycles)} not initialized");


        private int CycleId => testCycleWasInit
               ? TestCycle.Id
               : TestCycles.First().Id;


        private Day testDay;
        private bool testDayWasInit;
        internal Day TestDay => testDayWasInit
            ? testDay
            : throw new InvalidOperationException($"{nameof(testDay)} not initialized");


        private List<Day> testDays;
        private bool testDaysWasInit;
        internal List<Day> TestDays => testDaysWasInit
            ? testDays
            : throw new InvalidOperationException($"{nameof(testDays)} not initialized");


        private int DayId => testDayWasInit
               ? TestDay.Id
               : TestDays.First().Id;


        private Exercise testExercise;
        private bool testExerciseWasInit;
        internal Exercise TestExercise => testExerciseWasInit
            ? testExercise
            : throw new InvalidOperationException($"{nameof(testExercise)} not initialized");


        private List<Exercise> testExercises;
        private bool testExercisesWasInit;
        internal List<Exercise> TestExercises => testExercisesWasInit
            ? testExercises
            : throw new InvalidOperationException($"{nameof(testExercises)} not initialized");


        private int ExerciseId => testExerciseWasInit
               ? TestExercise.Id
               : TestExercises.First().Id;


        private ResultExercise testResultExercise;
        private bool testResultExerciseWasInit;
        internal ResultExercise TestResultExercise => testResultExerciseWasInit
            ? testResultExercise
            : throw new InvalidOperationException($"{nameof(testResultExercise)} not initialized");


        private List<ResultExercise> testResultExercises;
        private bool testResultExercisesWasInit;
        internal List<ResultExercise> TestResultExercises => testResultExercisesWasInit
            ? testResultExercises
            : throw new InvalidOperationException($"{nameof(testResultExercises)} not initialized");


        private int ResultExerciseId => testResultExerciseWasInit
               ? TestResultExercise.Id
               : TestResultExercises.First().Id;

        internal EntityContextBuilder()
        {
            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            options = new DbContextOptionsBuilder<EntityContext>()
                .UseSqlite(connection)
                .Options;
        }

        internal EntityContext Build()
        {
            if (!contextWasInit)
                throw new InvalidOperationException($"The context was not created.");

            return context;
        }

        internal EntityContextBuilder Create()
        {
            context = new EntityContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            contextWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithUserInformation()
        {
            testUserInformation = new UserInformation
            {
                FirstName = "TestFirstName",
                Username = "@TestUsername"
            };

            Context.UsersInformation.Add(testUserInformation);
            Context.SaveChanges();

            testUserInformationWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithUserInformations()
        {
            testUserInformations =
            [
                new UserInformation() { FirstName = "TestFirstName1", Username = "@TestUsername1" },
                new UserInformation() { FirstName = "TestFirstName2", Username = "@TestUsername2" },
            ];

            Context.UsersInformation.AddRange(testUserInformations);
            Context.SaveChanges();

            testUserInformationsWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithCycle()
        {
            testCycle = new Cycle() { Name = "TestCycle", UserInformationId = UserInformationId };

            Context.Cycles.Add(testCycle);
            Context.SaveChanges();

            testCycleWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithCycles()
        {
            testCycles =
           [
                new Cycle() { Name = "TestExercise1", UserInformationId = UserInformationId },
                new Cycle() { Name = "TestExercise2", UserInformationId = UserInformationId },
           ];

            Context.Cycles.AddRange(testCycles);
            Context.SaveChanges();

            testCyclesWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithCyclesOnOtherUserInformations()
        {
            testCycles =
           [
                new Cycle() { Name = "TestExercise1", UserInformationId = TestUserInformations[0].Id },
                new Cycle() { Name = "TestExercise2", UserInformationId = TestUserInformations[1].Id },
           ];

            Context.Cycles.AddRange(testCycles);
            Context.SaveChanges();

            testCyclesWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithDay()
        {
            testDay = new Day() { Name = "TestDay", CycleId = CycleId };

            Context.Days.Add(testDay);
            Context.SaveChanges();

            testDayWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithDays()
        {
            testDays =
           [
                new Day() { Name = "TestDay1", CycleId = CycleId },
                new Day() { Name = "TestDay2", CycleId = CycleId },
           ];

            Context.Days.AddRange(testDays);
            Context.SaveChanges();

            testDaysWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithExercise()
        {
            testExercise = new Exercise() { Name = "TestExercise1", Mode = ExercisesMods.Count, DayId = DayId };

            Context.Exercises.Add(testExercise);
            Context.SaveChanges();

            testExerciseWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithExercises()
        {
            testExercises =
           [
               new Exercise() { Name = "TestExercise1", Mode = ExercisesMods.Count, DayId = DayId },
               new Exercise() { Name = "TestExercise2", Mode = ExercisesMods.WeightCount, DayId = DayId },
               new Exercise() { Name = "TestExercise3", Mode = ExercisesMods.FreeResult, DayId = DayId }
           ];

            Context.Exercises.AddRange(testExercises);
            Context.SaveChanges();

            testExercisesWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithResultExercise()
        {
            testResultExercise = new ResultExercise() { Count = 20, ExerciseId = ExerciseId };

            Context.ResultsExercises.Add(testResultExercise);
            Context.SaveChanges();

            testResultExerciseWasInit = true;

            return this;
        }

        internal EntityContextBuilder WithResultExercises()
        {
            testResultExercises = 
           [
                new ResultExercise() { Count = 20, ExerciseId = ExerciseId },
                new ResultExercise() { Count = 40, ExerciseId = ExerciseId },
                new ResultExercise() { Count = 60, ExerciseId = ExerciseId },
           ];

            Context.ResultsExercises.AddRange(testResultExercises);
            Context.SaveChanges();

            testResultExercisesWasInit = true;

            return this;
        }

        public void Dispose()
        {
            context?.Dispose();
            connection?.Dispose();
        }
    }
}