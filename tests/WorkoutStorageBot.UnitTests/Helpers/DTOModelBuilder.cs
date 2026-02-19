#nullable disable

﻿using WorkoutStorageBot.Model.DTO.BusinessLogic;
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


        private List<DTOCycle> dtoTestCycles;
        private bool dtoTestCyclesWasInit;
        internal List<DTOCycle> DTOTestCycles => dtoTestCyclesWasInit
            ? dtoTestCycles
            : throw new InvalidOperationException($"{nameof(dtoTestCycles)} not initialized");


        private DTODay dtoTestDay;
        private bool dtoTestDayWasInit;
        internal DTODay DTOTestDay => dtoTestDayWasInit
            ? dtoTestDay
            : throw new InvalidOperationException($"{nameof(dtoTestDay)} not initialized");


        private List<DTODay> dtoTestDays;
        private bool dtoTestDaysWasInit;
        internal List<DTODay> DTOTestDays => dtoTestDaysWasInit
            ? dtoTestDays
            : throw new InvalidOperationException($"{nameof(dtoTestDays)} not initialized");


        private DTOExercise dtoTestExercise;
        private bool dtoTestExerciseWasInit;
        internal DTOExercise DTOTestExercise => dtoTestExerciseWasInit
            ? dtoTestExercise
            : throw new InvalidOperationException($"{nameof(dtoTestExercise)} not initialized");


        private List<DTOExercise> dtoTestExercises;
        private bool dtoTestExercisesWasInit;
        internal List<DTOExercise> DTOTestExercises => dtoTestExercisesWasInit
            ? dtoTestExercises
            : throw new InvalidOperationException($"{nameof(dtoTestExercises)} not initialized");


        private DTOResultExercise dtoTestResultExercise;
        private bool dtoTestResultExerciseWasInit;
        internal DTOResultExercise DTOTestResultExercise => dtoTestResultExerciseWasInit
            ? dtoTestResultExercise
            : throw new InvalidOperationException($"{nameof(dtoTestResultExercise)} not initialized");


        private List<DTOResultExercise> dtoTestResultExercises;
        private bool dtoTestResultExercisesWasInit;
        internal List<DTOResultExercise> DTOTestResultExercises => dtoTestResultExercisesWasInit
            ? dtoTestResultExercises
            : throw new InvalidOperationException($"{nameof(dtoTestResultExercises)} not initialized");


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

        internal DTOModelBuilder WithDTOCyclesByDTOUserInformation()
        {
            dtoTestCycles =
           [
                new DTOCycle() { Id = 1, Name = "DTOCycle1", UserInformation = DTOTestUserInformation, UserInformationId = DTOTestUserInformation.Id },
                new DTOCycle() { Id = 2, Name = "DTOCycle2", UserInformation = DTOTestUserInformation, UserInformationId = DTOTestUserInformation.Id },
           ];

            dtoTestCyclesWasInit = true;

            DTOTestUserInformation.Cycles = DTOTestCycles;

            return this;
        }

        internal DTOModelBuilder WithDTOCycles()
        {
            dtoTestCycles =
           [
                new DTOCycle() { Id = 1, Name = "DTOCycle1" },
                new DTOCycle() { Id = 2, Name = "DTOCycle2" },
           ];

            dtoTestCyclesWasInit = true;

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

        internal DTOModelBuilder WithDTODaysByDTOCycle()
        {
            dtoTestDays =
           [
                new DTODay() { Id = 1, Name = "DTODay1", Cycle = DTOTestCycle, CycleId = DTOTestCycle.Id },
                new DTODay() { Id = 2, Name = "DTODay2", Cycle = DTOTestCycle, CycleId = DTOTestCycle.Id },
           ];

            dtoTestDaysWasInit = true;

            DTOTestCycle.Days = DTOTestDays;

            return this;
        }

        internal DTOModelBuilder WithDTODays()
        {
            dtoTestDays =
           [
                new DTODay() { Id = 1, Name = "DTODay1" },
                new DTODay() { Id = 2, Name = "DTODay2" },
           ];

            dtoTestDaysWasInit = true;

            return this;
        }

        internal DTOModelBuilder WithDTOExerciseByDTODay()
        {
            dtoTestExercise = new DTOExercise
            {
                Id = 100,
                Name = "Exercise 1",
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
                Name = "Exercise 1",
                Mode = ExercisesMods.Count,
                IsArchive = true
            };

            dtoTestExerciseWasInit = true;

            return this;
        }

        internal DTOModelBuilder WithDTOExercisesByDTODay()
        {
            dtoTestExercises =
           [
                new DTOExercise() { Id = 1, Name = "DTOExercise1", Mode = ExercisesMods.Count, Day = DTOTestDay, DayId = DTOTestDay.Id },
                new DTOExercise() { Id = 2, Name = "DTOExercise2", Mode = ExercisesMods.WeightCount, Day = DTOTestDay, DayId = DTOTestDay.Id },
                new DTOExercise() { Id = 3, Name = "DTOExercise3", Mode = ExercisesMods.Timer, Day = DTOTestDay, DayId = DTOTestDay.Id },
                new DTOExercise() { Id = 4, Name = "DTOExercise4", Mode = ExercisesMods.FreeResult, Day = DTOTestDay, DayId = DTOTestDay.Id },
           ];

            dtoTestExercisesWasInit = true;

            DTOTestDay.Exercises = DTOTestExercises;

            return this;
        }

        internal DTOModelBuilder WithDTOExercises()
        {
            dtoTestExercises =
           [
                new DTOExercise() { Id = 1, Name = "DTOExercise1", Mode = ExercisesMods.Count },
                new DTOExercise() { Id = 2, Name = "DTOExercise2", Mode = ExercisesMods.WeightCount },
                new DTOExercise() { Id = 3, Name = "DTOExercise3", Mode = ExercisesMods.Timer },
                new DTOExercise() { Id = 4, Name = "DTOExercise4", Mode = ExercisesMods.FreeResult },
           ];

            dtoTestExercisesWasInit = true;

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

        internal DTOModelBuilder WithDTOResultExercisesByDTOExercise()
        {
            dtoTestResultExercises =
           [
                new DTOResultExercise() { Id = 1, Count = 10, Weight = 20, FreeResult = "DTOResultExercise1", DateTime = DateTime.Now, Exercise = DTOTestExercise, ExerciseId = DTOTestExercise.Id },
                new DTOResultExercise() { Id = 2, Count = 20, Weight = 40, FreeResult = "DTOResultExercise2", DateTime = DateTime.Now.AddMinutes(1), Exercise = DTOTestExercise, ExerciseId = DTOTestExercise.Id },
                new DTOResultExercise() { Id = 3, Count = 30, Weight = 60, FreeResult = "DTOResultExercise3", DateTime = DateTime.Now.AddMinutes(2), Exercise = DTOTestExercise, ExerciseId = DTOTestExercise.Id },
                new DTOResultExercise() { Id = 4, Count = 40, Weight = 80, FreeResult = "DTOResultExercise4", DateTime = DateTime.Now.AddMinutes(3), Exercise = DTOTestExercise, ExerciseId = DTOTestExercise.Id },
           ];

            dtoTestResultExercisesWasInit = true;

            DTOTestExercise.ResultsExercise = DTOTestResultExercises;

            return this;
        }

        internal DTOModelBuilder WithDTOResultExercises()
        {
            dtoTestResultExercises =
           [
                new DTOResultExercise() { Id = 1, Count = 10, Weight = 20, FreeResult = "DTOResultExercise1", DateTime = DateTime.Now },
                new DTOResultExercise() { Id = 2, Count = 20, Weight = 40, FreeResult = "DTOResultExercise2", DateTime = DateTime.Now.AddMinutes(1) },
                new DTOResultExercise() { Id = 3, Count = 30, Weight = 60, FreeResult = "DTOResultExercise3", DateTime = DateTime.Now.AddMinutes(2) },
                new DTOResultExercise() { Id = 4, Count = 40, Weight = 80, FreeResult = "DTOResultExercise4", DateTime = DateTime.Now.AddMinutes(3) },
           ];

            dtoTestResultExercisesWasInit = true;

            return this;
        }
    }
}