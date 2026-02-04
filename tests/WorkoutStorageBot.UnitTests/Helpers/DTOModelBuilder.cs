using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.Helpers
{
    internal class DTOModelBuilder
    {
        private DTOUserInformation dtoTestUserInformation;
        private bool dtoTestUserInformationWasInit;
        internal DTOUserInformation DTOTestUserInformation => dtoTestUserInformationWasInit
            ? dtoTestUserInformation
            : throw new InvalidOperationException($"{nameof(dtoTestUserInformation)} not initialized");


        private DTOCycle dtoTestCycle;
        private bool dtoTestCycleWasInit;
        internal DTOCycle DTOTestCycle => dtoTestCycleWasInit
            ? dtoTestCycle
            : throw new InvalidOperationException($"{nameof(dtoTestCycle)} not initialized");


        private DTODay dtoTestDay;
        private bool dtoTestDayWasInit;
        internal DTODay DTOTestDay => dtoTestDayWasInit
            ? dtoTestDay
            : throw new InvalidOperationException($"{nameof(dtoTestDay)} not initialized");


        private DTOExercise dtoTestExercise;
        private bool dtoTestExerciseWasInit;
        internal DTOExercise DTOTestExercise => dtoTestExerciseWasInit
            ? dtoTestExercise
            : throw new InvalidOperationException($"{nameof(dtoTestExercise)} not initialized");


        private DTOResultExercise dtoTestResultExercise;
        private bool dtoTestResultExerciseWasInit;
        internal DTOResultExercise DTOTestResultExercise => dtoTestResultExerciseWasInit
            ? dtoTestResultExercise
            : throw new InvalidOperationException($"{nameof(dtoTestResultExercise)} not initialized");

        internal DTOModelBuilder()
        {

        }

        internal DTOModelBuilder WithDTOUserInformation()
        {
            dtoTestUserInformation = new DTOUserInformation
            {
                Id = 1,
                UserId = 12345,
                FirstName = "John",
                Username = "johndoe",
                WhiteList = true,
                BlackList = true
            };

            dtoTestUserInformationWasInit = true;

            return this;
        }

        internal DTOModelBuilder WithDTOCycleByDTOUserInformation()
        {
            dtoTestCycle = new DTOCycle
            {
                Id = 10,
                Name = "Cycle 1",
                UserInformation = dtoTestUserInformation,
                UserInformationId = dtoTestUserInformation.Id,
                IsActive = true,
                IsArchive = true
            };

            dtoTestCycleWasInit = true;

            dtoTestUserInformation.Cycles = new List<DTOCycle> { DTOTestCycle };

            return this;
        }

        internal DTOModelBuilder WithDTOCycle()
        {
            dtoTestCycle = new DTOCycle
            {
                Id = 10,
                Name = "Cycle 1",
                IsActive = true,
                IsArchive = true
            };

            dtoTestCycleWasInit = true;

            return this;
        }

        internal DTOModelBuilder WithDTODayByDTOCycle()
        {
            dtoTestDay = new DTODay
            {
                Id = 100,
                Name = "Day 1",
                Cycle = DTOTestCycle,
                CycleId = DTOTestCycle.Id,
                IsArchive = true
            };

            dtoTestDayWasInit = true;

            DTOTestCycle.Days = new List<DTODay> { DTOTestDay };

            return this;
        }

        internal DTOModelBuilder WithDTODay()
        {
            dtoTestDay = new DTODay
            {
                Id = 100,
                Name = "Day 1",
                IsArchive = true
            };

            dtoTestDayWasInit = true;

            return this;
        }

        internal DTOModelBuilder WithDTOExerciseByDTODay()
        {
            dtoTestExercise = new DTOExercise
            {
                Id = 100,
                Name = "Day 1",
                Mode = ExercisesMods.Count,
                Day = DTOTestDay,
                DayId = DTOTestDay.Id,
                IsArchive = true
            };

            dtoTestExerciseWasInit = true;

            DTOTestDay.Exercises = new List<DTOExercise> { DTOTestExercise };

            return this;
        }

        internal DTOModelBuilder WithDTOExercise()
        {
            dtoTestExercise = new DTOExercise
            {
                Id = 100,
                Name = "Day 1",
                Mode = ExercisesMods.Count,
                IsArchive = true
            };

            dtoTestExerciseWasInit = true;

            return this;
        }

        internal DTOModelBuilder WithDTOResultExerciseByDTOExercise()
        {
            dtoTestResultExercise = new DTOResultExercise
            {
                Id = 100,
                Count = 1,
                Weight = 1,
                DateTime = DateTime.Now,
                Exercise = DTOTestExercise,
                ExerciseId = DTOTestExercise.Id,
            };

            dtoTestResultExerciseWasInit = true;

            DTOTestExercise.ResultsExercise = new List<DTOResultExercise> { DTOTestResultExercise };

            return this;
        }

        internal DTOModelBuilder WithDTOResultExercise()
        {
            dtoTestResultExercise = new DTOResultExercise
            {
                Id = 100,
                Count = 1,
                Weight = 1,
                DateTime = DateTime.Now,
            };

            dtoTestResultExerciseWasInit = true;

            return this;
        }
    }
}