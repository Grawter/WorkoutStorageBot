#nullable disable

using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.Helpers
{
    internal class DomainModelBuilder
    {
        private UserInformation testUserInformation;
        private bool testUserInformationWasInit;
        internal UserInformation TestUserInformation => testUserInformationWasInit
            ? testUserInformation
            : throw new InvalidOperationException($"{nameof(testUserInformation)} not initialized");


        private Cycle testCycle;
        private bool testCycleWasInit;
        internal Cycle TestCycle => testCycleWasInit
            ? testCycle
            : throw new InvalidOperationException($"{nameof(testCycle)} not initialized");


        private Day testDay;
        private bool testDayWasInit;
        internal Day TestDay => testDayWasInit
            ? testDay
            : throw new InvalidOperationException($"{nameof(testDay)} not initialized");


        private Exercise testExercise;
        private bool testExerciseWasInit;
        internal Exercise TestExercise => testExerciseWasInit
            ? testExercise
            : throw new InvalidOperationException($"{nameof(testExercise)} not initialized");


        private ResultExercise testResultExercise;
        private bool testResultExerciseWasInit;
        internal ResultExercise TestResultExercise => testResultExerciseWasInit
            ? testResultExercise
            : throw new InvalidOperationException($"{nameof(testResultExercise)} not initialized");


        internal DomainModelBuilder()
        {

        }

        internal DomainModelBuilder WithUserInformation()
        {
            testUserInformation = new UserInformation
            {
                Id = 1,
                UserId = 12345,
                FirstName = "John",
                Username = "johndoe",
                WhiteList = true,
                BlackList = true
            };

            testUserInformationWasInit = true;

            return this;
        }

        internal DomainModelBuilder WithCycleByUserInformation()
        {
            testCycle = new Cycle
            {
                Id = 10,
                Name = "Cycle 1",
                UserInformation = TestUserInformation,
                UserInformationId = TestUserInformation.Id,
                IsActive = true,
                IsArchive = true
            };

            testCycleWasInit = true;

            TestUserInformation.Cycles = new List<Cycle> { TestCycle };

            return this;
        }

        internal DomainModelBuilder WithCycle()
        {
            testCycle = new Cycle
            {
                Id = 10,
                Name = "Cycle 1",
                IsActive = true,
                IsArchive = true
            };

            testCycleWasInit = true;

            return this;
        }

        internal DomainModelBuilder WithDayByCycle()
        {
            testDay = new Day
            {
                Id = 100,
                Name = "Day 1",
                Cycle = TestCycle,
                CycleId = TestCycle.Id,
                IsArchive = true
            };

            testDayWasInit = true;

            TestCycle.Days = new List<Day> { TestDay };

            return this;
        }

        internal DomainModelBuilder WithDay()
        {
            testDay = new Day
            {
                Id = 100,
                Name = "Day 1",
                IsArchive = true
            };

            testDayWasInit = true;

            return this;
        }

        internal DomainModelBuilder WithExerciseByDay()
        {
            testExercise = new Exercise
            {
                Id = 100,
                Name = "Exercise 1",
                Mode = ExercisesMods.Count,
                Day = TestDay,
                DayId = TestDay.Id,
                IsArchive = true
            };

            testExerciseWasInit = true;

            TestDay.Exercises = new List<Exercise> { TestExercise };

            return this;
        }

        internal DomainModelBuilder WithExercise()
        {
            testExercise = new Exercise
            {
                Id = 100,
                Name = "Exercise 1",
                Mode = ExercisesMods.Count,
                IsArchive = true
            };

            testExerciseWasInit = true;

            return this;
        }

        internal DomainModelBuilder WithResultExerciseByExercise()
        {
            testResultExercise = new ResultExercise
            {
                Id = 100,
                Count = 1,
                Weight = 1,
                DateTime = DateTime.Now,
                Exercise = TestExercise,
                ExerciseId = TestExercise.Id,
            };

            testResultExerciseWasInit = true;

            TestExercise.ResultsExercise = new List<ResultExercise> { TestResultExercise };

            return this;
        }

        internal DomainModelBuilder WithResultExercise()
        {
            testResultExercise = new ResultExercise
            {
                Id = 100,
                Count = 1,
                Weight = 1,
                DateTime = DateTime.Now,
            };

            testResultExerciseWasInit = true;

            return this;
        }
    }
}