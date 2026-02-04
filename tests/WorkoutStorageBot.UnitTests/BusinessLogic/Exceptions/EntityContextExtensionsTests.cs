using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Interfaces;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Exceptions
{
    public class EntityContextExtensionsTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntity_WithDTO_ShouldSaveCycle(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation();
            EntityContext context = entityContextBuilder.Build();

            IDTODomain dtoCycle = new DTOCycle() { UserInformationId = entityContextBuilder.TestUserInformation.Id, Name = "TestCycle" };

            // Act
            await context.AddEntity(dtoCycle, isNeedSaveChanges);

            Cycle? savedCycle = context.Cycles.Find(dtoCycle.Id);

            // Assert
            if (isNeedSaveChanges)
            {
                savedCycle.Should().NotBeNull();
                savedCycle.Name.Should().Be(dtoCycle.Name);

                dtoCycle.Id.Should().NotBe(0);
            }
            else
            {
                savedCycle.Should().BeNull();

                dtoCycle.Id.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntity_WithDTO_ShouldSaveDay(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle();
            EntityContext context = entityContextBuilder.Build();

            IDTODomain dtoDay = new DTODay() { CycleId = entityContextBuilder.TestUserInformation.Id, Name = "TestDay" };

            // Act
            await context.AddEntity(dtoDay, isNeedSaveChanges);

            Day? savedDay = context.Days.Find(dtoDay.Id);

            // Assert
            if (isNeedSaveChanges)
            {
                savedDay.Should().NotBeNull();
                savedDay.Name.Should().Be(dtoDay.Name);

                dtoDay.Id.Should().NotBe(0);
            }
            else
            {
                savedDay.Should().BeNull();

                dtoDay.Id.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntity_WithDTO_ShouldSaveExercise(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay();
            EntityContext context = entityContextBuilder.Build();

            IDTODomain dtoExercise = new DTOExercise() { DayId = entityContextBuilder.TestDay.Id, Name = "TestExercise", Mode = ExercisesMods.Count };

            // Act
            await context.AddEntity(dtoExercise, isNeedSaveChanges);

            Exercise? savedExercise = context.Exercises.Find(dtoExercise.Id);

            // Assert
            if (isNeedSaveChanges)
            {
                savedExercise.Should().NotBeNull();
                savedExercise.Name.Should().Be(dtoExercise.Name);

                dtoExercise.Id.Should().NotBe(0);
            }
            else
            {
                savedExercise.Should().BeNull();

                dtoExercise.Id.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntity_WithDTO_ShouldSaveResultExercise(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercise();
            EntityContext context = entityContextBuilder.Build();

            DTOResultExercise dtoResultExercise = new DTOResultExercise() 
            {
                ExerciseId = entityContextBuilder.TestExercise.Id, 
                Count = 5,
                Weight = 20,
                DateTime = DateTime.Now,
                FreeResult = "ABC",
            };

            // Act
            await context.AddEntity(dtoResultExercise, isNeedSaveChanges);

            ResultExercise? savedResultExercise = context.ResultsExercises.Find(dtoResultExercise.Id);

            // Assert
            if (isNeedSaveChanges)
            {
                savedResultExercise.Should().NotBeNull();
                savedResultExercise.Count.Should().Be(dtoResultExercise.Count);
                savedResultExercise.DateTime.Should().Be(dtoResultExercise.DateTime);
                savedResultExercise.FreeResult.Should().Be(dtoResultExercise.FreeResult);

                dtoResultExercise.Id.Should().NotBe(0);
            }
            else
            {
                savedResultExercise.Should().BeNull();

                dtoResultExercise.Id.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntities_WithDTO_ShouldSaveCycles(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation();
                                                                                        
            EntityContext context = entityContextBuilder.Build();

            List<DTOCycle> dtoCycles =
            [
                new DTOCycle() { Name = "TestCycle1", UserInformationId = entityContextBuilder.TestUserInformation.Id },
                new DTOCycle() { Name = "TestCycle2", UserInformationId = entityContextBuilder.TestUserInformation.Id },
            ];

            // Act
            await context.AddEntities(dtoCycles, isNeedSaveChanges);

            // Assert
            for (int i = 0; i < dtoCycles.Count; i++)
            {
                int id = i + 1;

                Cycle? savedCycle = context.Cycles.Find(id);

                if (isNeedSaveChanges)
                {
                    savedCycle.Should().NotBeNull();
                    savedCycle.Name.Should().Be($"TestCycle{id}");

                    dtoCycles[i].Id.Should().Be(id);
                }
                else
                {
                    savedCycle.Should().BeNull();

                    dtoCycles[i].Id.Should().Be(0);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntities_WithDTO_ShouldSaveDays(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle();

            EntityContext context = entityContextBuilder.Build();

            List<DTODay> dtoDays =
            [
                new DTODay() { Name = "TestDay1", CycleId = entityContextBuilder.TestCycle.Id },
                new DTODay() { Name = "TestDay2", CycleId = entityContextBuilder.TestCycle.Id },
            ];

            // Act
            await context.AddEntities(dtoDays, isNeedSaveChanges);

            // Assert
            for (int i = 0; i < dtoDays.Count; i++)
            {
                int id = i + 1;

                Day? savedDay = context.Days.Find(id);

                if (isNeedSaveChanges)
                {
                    savedDay.Should().NotBeNull();
                    savedDay.Name.Should().Be($"TestDay{id}");

                    dtoDays[i].Id.Should().Be(id);
                }
                else
                {
                    savedDay.Should().BeNull();

                    dtoDays[i].Id.Should().Be(0);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntities_WithDTO_ShouldSaveExercises(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay();

            EntityContext context = entityContextBuilder.Build();

            List<DTOExercise> dtoExercises =
            [
                new DTOExercise() { Name = "TestExercise1", Mode = ExercisesMods.Count, DayId = entityContextBuilder.TestDay.Id },
                new DTOExercise() { Name = "TestExercise2", Mode = ExercisesMods.Count, DayId = entityContextBuilder.TestDay.Id },
            ];

            // Act
            await context.AddEntities(dtoExercises, isNeedSaveChanges);

            // Assert
            for (int i = 0; i < dtoExercises.Count; i++)
            {
                int id = i + 1;

                Exercise? savedExercise= context.Exercises.Find(id);

                if (isNeedSaveChanges)
                {
                    savedExercise.Should().NotBeNull();
                    savedExercise.Name.Should().Be($"TestExercise{id}");

                    dtoExercises[i].Id.Should().Be(id);
                }
                else
                {
                    savedExercise.Should().BeNull();

                    dtoExercises[i].Id.Should().Be(0);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddEntities_WithDTO_ShouldSaveResultExercises(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercise();

            EntityContext context = entityContextBuilder.Build();

            List<DTOResultExercise> dtoResultExercises =
            [
                new DTOResultExercise() { Count = 1, ExerciseId = entityContextBuilder.TestExercise.Id },
                new DTOResultExercise() { Count = 2, ExerciseId = entityContextBuilder.TestExercise.Id  },
            ];

            // Act
            await context.AddEntities(dtoResultExercises, isNeedSaveChanges);

            // Assert
            for (int i = 0; i < dtoResultExercises.Count; i++)
            {
                int id = i + 1;

                ResultExercise? savedResultExercise = context.ResultsExercises.Find(id);

                if (isNeedSaveChanges)
                {
                    savedResultExercise.Should().NotBeNull();
                    savedResultExercise.Count.Should().Be(id);

                    dtoResultExercises[i].Id.Should().Be(id);
                }
                else
                {
                    savedResultExercise.Should().BeNull();

                    dtoResultExercises[i].Id.Should().Be(0);
                }
            }
        }

        [Fact]
        public async Task UpdateEntity__WithDTO_ShouldUpdateCycle()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformations()
                                                                                        .WithCycle();

            EntityContext context = entityContextBuilder.Build();

            Cycle updatedCycle = entityContextBuilder.TestCycle;

            DTOCycle dtoCycle = updatedCycle.ToDTOCycle();

            dtoCycle.UserInformationId = entityContextBuilder.TestUserInformations[1].Id;
            dtoCycle.Name = "UpdatedTestCycle";
            dtoCycle.IsActive = true;
            dtoCycle.IsArchive = true;

            // Act
            await context.UpdateEntity(dtoCycle, true);

            // Assert
            updatedCycle.UserInformationId.Should().Be(dtoCycle.UserInformationId);
            updatedCycle.Name.Should().Be(dtoCycle.Name);
            updatedCycle.IsActive.Should().Be(dtoCycle.IsActive);
            updatedCycle.IsArchive.Should().Be(dtoCycle.IsArchive);
        }

        [Fact]
        public async Task UpdateEntity_WithDTO_ShouldUpdateDay()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycles()
                                                                                        .WithDay();

            EntityContext context = entityContextBuilder.Build();

            Day updatedDay = entityContextBuilder.TestDay;

            DTODay dtoDay = updatedDay.ToDTODay();

            dtoDay.CycleId = entityContextBuilder.TestCycles[1].Id;
            dtoDay.Name = "UpdatedTestDay";
            dtoDay.IsArchive = true;

            // Act
            await context.UpdateEntity(dtoDay, true);

            // Assert
            updatedDay.CycleId.Should().Be(dtoDay.CycleId);
            updatedDay.Name.Should().Be(dtoDay.Name);
            updatedDay.IsArchive.Should().Be(dtoDay.IsArchive);
        }

        [Fact]
        public async Task UpdateEntity_WithDTO_ShouldUpdateExercise()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDays()
                                                                                        .WithExercise();

            EntityContext context = entityContextBuilder.Build();

            Exercise updatedExercise = entityContextBuilder.TestExercise;

            DTOExercise dtoExercise = updatedExercise.ToDTOExercise();

            dtoExercise.DayId = entityContextBuilder.TestDays[1].Id;
            dtoExercise.Name = "UpdatedTestExercise";
            dtoExercise.IsArchive = true;

            // Act
            await context.UpdateEntity(dtoExercise, true);

            // Assert
            updatedExercise.DayId.Should().Be(dtoExercise.DayId);
            updatedExercise.Name.Should().Be(dtoExercise.Name);
            updatedExercise.IsArchive.Should().Be(dtoExercise.IsArchive);
        }

        [Fact]
        public async Task UpdateEntity_WithDTO_ShouldUpdateResultExercise()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises()
                                                                                        .WithResultExercise();

            EntityContext context = entityContextBuilder.Build();

            ResultExercise updatedResultExercise = entityContextBuilder.TestResultExercise;

            DTOResultExercise dtoResultExercise = updatedResultExercise.ToDTOResultExercise();

            dtoResultExercise.ExerciseId = entityContextBuilder.TestExercises[1].Id;
            dtoResultExercise.Count = 35;
            dtoResultExercise.Weight = 25;
            dtoResultExercise.DateTime = DateTime.Now;
            dtoResultExercise.FreeResult = "ABC";

            // Act
            await context.UpdateEntity(dtoResultExercise, true);

            // Assert
            updatedResultExercise.ExerciseId.Should().Be(dtoResultExercise.ExerciseId);
            updatedResultExercise.Count.Should().Be(dtoResultExercise.Count);
            updatedResultExercise.Weight.Should().Be(dtoResultExercise.Weight);
            updatedResultExercise.DateTime.Should().Be(dtoResultExercise.DateTime);
            updatedResultExercise.FreeResult.Should().Be(dtoResultExercise.FreeResult);
        }

        [Fact]
        public async Task UpdateEntities_WithDTO_ShouldUpdateCycles()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformations()
                                                                                        .WithCycles();

            EntityContext context = entityContextBuilder.Build();

            List<DTOCycle> dtoCycles = new List<DTOCycle>();

            for (int i = 0; i < entityContextBuilder.TestCycles.Count; i++)
            {
                Cycle updatedCycle = entityContextBuilder.TestCycles[i];

                DTOCycle dtoCycle = updatedCycle.ToDTOCycle();

                dtoCycle.UserInformationId = entityContextBuilder.TestUserInformations[i == 0 ? 1 : 0].Id;
                dtoCycle.Name = $"UpdatedTestCycle{i}";
                dtoCycle.IsActive = true;
                dtoCycle.IsArchive = true;

                dtoCycles.Add(dtoCycle);
            }

            // Act
            await context.UpdateEntities(dtoCycles, true);

            // Assert
            for (int i = 0; i < entityContextBuilder.TestCycles.Count; i++)
            {
                Cycle updatedCycle = entityContextBuilder.TestCycles[i];

                DTOCycle dtoCycle = dtoCycles[i];

                updatedCycle.UserInformationId.Should().Be(dtoCycle.UserInformationId);
                updatedCycle.Name.Should().Be(dtoCycle.Name);
                updatedCycle.IsActive.Should().Be(dtoCycle.IsActive);
                updatedCycle.IsArchive.Should().Be(dtoCycle.IsArchive);
            }
        }

        [Fact]
        public async Task UpdateEntities_WithDTO_ShouldUpdateDays()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycles()
                                                                                        .WithDays();

            EntityContext context = entityContextBuilder.Build();

            List<DTODay> dtoDays = new List<DTODay>();

            for (int i = 0; i < entityContextBuilder.TestDays.Count; i++)
            {
                Day updatedDay = entityContextBuilder.TestDays[i];

                DTODay dtoDay = updatedDay.ToDTODay();

                dtoDay.CycleId = entityContextBuilder.TestCycles[i == 0 ? 1 : 0].Id;
                dtoDay.Name = $"UpdatedTestDay{i}";
                dtoDay.IsArchive = true;

                dtoDays.Add(dtoDay);
            }

            // Act
            await context.UpdateEntities(dtoDays, true);

            // Assert
            for (int i = 0; i < entityContextBuilder.TestDays.Count; i++)
            {
                Day updatedDay = entityContextBuilder.TestDays[i];

                DTODay dtoDay = dtoDays[i];

                updatedDay.CycleId.Should().Be(dtoDay.CycleId);
                updatedDay.Name.Should().Be(dtoDay.Name);
                updatedDay.IsArchive.Should().Be(dtoDay.IsArchive);
            }
        }

        [Fact]
        public async Task UpdateEntities_WithDTO_ShouldUpdateExercises()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDays()
                                                                                        .WithExercises();

            EntityContext context = entityContextBuilder.Build();

            List<DTOExercise> dtoExercises = new List<DTOExercise>();

            for (int i = 0; i < entityContextBuilder.TestExercises.Count; i++)
            {
                Exercise updatedExercise = entityContextBuilder.TestExercises[i];

                DTOExercise dtoExercise = updatedExercise.ToDTOExercise();

                dtoExercise.DayId = entityContextBuilder.TestDays[i == 0 ? 1 : 0].Id;
                dtoExercise.Name = $"UpdatedTestExercise{i}";
                dtoExercise.IsArchive = true;

                dtoExercises.Add(dtoExercise);
            }

            // Act
            await context.UpdateEntities(dtoExercises, true);

            // Assert
            for (int i = 0; i < entityContextBuilder.TestExercises.Count; i++)
            {
                Exercise updatedExercise = entityContextBuilder.TestExercises[i];

                DTOExercise dtoExercise = dtoExercises[i];

                updatedExercise.DayId.Should().Be(dtoExercise.DayId);
                updatedExercise.Name.Should().Be(dtoExercise.Name);
                updatedExercise.IsArchive.Should().Be(dtoExercise.IsArchive);
            }
        }

        [Fact]
        public async Task UpdateEntities_WithDTO_ShouldUpdateResultExercises()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises()
                                                                                        .WithResultExercises();

            EntityContext context = entityContextBuilder.Build();

            List<DTOResultExercise> dtoResultExercises = new List<DTOResultExercise>();

            for (int i = 0; i < entityContextBuilder.TestResultExercises.Count; i++)
            {
                ResultExercise updatedResultExercise = entityContextBuilder.TestResultExercises[i];

                DTOResultExercise dtoResultExercise = updatedResultExercise.ToDTOResultExercise();

                dtoResultExercise.ExerciseId = entityContextBuilder.TestExercises[i == 0 ? 1 : 0].Id;
                dtoResultExercise.Count = 1 + i;
                dtoResultExercise.Weight = 2 + i;
                dtoResultExercise.DateTime = DateTime.Now;
                dtoResultExercise.FreeResult = $"ABC{i}";

                dtoResultExercises.Add(dtoResultExercise);
            }

            // Act
            await context.UpdateEntities(dtoResultExercises, true);

            // Assert
            for (int i = 0; i < entityContextBuilder.TestResultExercises.Count; i++)
            {
                ResultExercise updatedResultExercise = entityContextBuilder.TestResultExercises[i];

                DTOResultExercise dtoResultExercise = dtoResultExercises[i];

                updatedResultExercise.ExerciseId.Should().Be(dtoResultExercise.ExerciseId);
                updatedResultExercise.Count.Should().Be(dtoResultExercise.Count);
                updatedResultExercise.Weight.Should().Be(dtoResultExercise.Weight);
                updatedResultExercise.DateTime.Should().Be(dtoResultExercise.DateTime);
                updatedResultExercise.FreeResult.Should().Be(dtoResultExercise.FreeResult);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntity_WithDTO_ShouldRemoveCycle(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle();

            EntityContext context = entityContextBuilder.Build();

            context.Entry(entityContextBuilder.TestCycle).State = EntityState.Detached;

            DTOCycle dtoCycle = entityContextBuilder.TestCycle.ToDTOCycle();

            // Act
            await context.RemoveEntity(dtoCycle, isNeedSaveChanges);

            Cycle? removedCycle = context.Cycles.Find(dtoCycle.Id);

            // Assert
            if (isNeedSaveChanges)
                removedCycle.Should().BeNull();
            else
                removedCycle.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntity_WithDTO_ShouldRemoveDay(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay();

            EntityContext context = entityContextBuilder.Build();

            context.Entry(entityContextBuilder.TestDay).State = EntityState.Detached;

            DTODay dtoDay = entityContextBuilder.TestDay.ToDTODay();

            // Act
            await context.RemoveEntity(dtoDay, isNeedSaveChanges);

            Day? removedDay = context.Days.Find(dtoDay.Id);

            // Assert
            if (isNeedSaveChanges)
                removedDay.Should().BeNull();
            else
                removedDay.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntity_WithDTO_ShouldRemoveExercise(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercise();

            EntityContext context = entityContextBuilder.Build();

            context.Entry(entityContextBuilder.TestExercise).State = EntityState.Detached;

            DTOExercise dtoExercise = entityContextBuilder.TestExercise.ToDTOExercise();

            // Act
            await context.RemoveEntity(dtoExercise, isNeedSaveChanges);

            Exercise? removedExercise = context.Exercises.Find(dtoExercise.Id);

            // Assert
            if (isNeedSaveChanges)
                removedExercise.Should().BeNull();
            else
                removedExercise.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntity_WithDTO_ShouldRemoveResultExercise(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercise()
                                                                                        .WithResultExercise();

            EntityContext context = entityContextBuilder.Build();

            context.Entry(entityContextBuilder.TestResultExercise).State = EntityState.Detached;

            DTOResultExercise dtoResultExercise = entityContextBuilder.TestResultExercise.ToDTOResultExercise();

            // Act
            await context.RemoveEntity(dtoResultExercise, isNeedSaveChanges);

            ResultExercise? removedResultExercise = context.ResultsExercises.Find(dtoResultExercise.Id);

            // Assert
            if (isNeedSaveChanges)
                removedResultExercise.Should().BeNull();
            else
                removedResultExercise.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntities_WithDTO_ShouldRemoveCycles(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycles();

            EntityContext context = entityContextBuilder.Build();

            List<DTOCycle> cyclesToRemove = new List<DTOCycle>();

            foreach (Cycle cycle in entityContextBuilder.TestCycles)
            {
                context.Entry(cycle).State = EntityState.Detached;

                DTOCycle dtoCycle = cycle.ToDTOCycle();

                cyclesToRemove.Add(dtoCycle);
            }

            // Act
            await context.RemoveEntities(cyclesToRemove, isNeedSaveChanges);

            foreach (DTOCycle removedDTOCycle in cyclesToRemove)
            {
                Cycle? removedCycle = context.Cycles.Find(removedDTOCycle.Id);

                // Assert
                if (isNeedSaveChanges)
                    removedCycle.Should().BeNull();
                else
                    removedCycle.Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntities_WithDTO_ShouldRemoveDays(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDays();

            EntityContext context = entityContextBuilder.Build();

            List<DTODay> daysToRemove = new List<DTODay>();

            foreach (Day day in entityContextBuilder.TestDays)
            {
                context.Entry(day).State = EntityState.Detached;

                DTODay dtoDay = day.ToDTODay();

                daysToRemove.Add(dtoDay);
            }

            // Act
            await context.RemoveEntities(daysToRemove, isNeedSaveChanges);

            foreach (DTODay removedDTODay in daysToRemove)
            {
                Day? removedDay = context.Days.Find(removedDTODay.Id);

                // Assert
                if (isNeedSaveChanges)
                    removedDay.Should().BeNull();
                else
                    removedDay.Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntities_WithDTO_ShouldRemoveExercises(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises();

            EntityContext context = entityContextBuilder.Build();

            List<DTOExercise> exercisesToRemove = new List<DTOExercise>();

            foreach (Exercise exercise in entityContextBuilder.TestExercises)
            {
                context.Entry(exercise).State = EntityState.Detached;

                DTOExercise dtoExercise = exercise.ToDTOExercise();

                exercisesToRemove.Add(dtoExercise);
            }

            // Act
            await context.RemoveEntities(exercisesToRemove, isNeedSaveChanges);

            foreach (DTOExercise removedDTOExercise in exercisesToRemove)
            {
                Exercise? removedExercise = context.Exercises.Find(removedDTOExercise.Id);

                // Assert
                if (isNeedSaveChanges)
                    removedExercise.Should().BeNull();
                else
                    removedExercise.Should().NotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveEntities_WithDTO_ShouldRemoveResultExercises(bool isNeedSaveChanges)
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercise()
                                                                                        .WithResultExercises();

            EntityContext context = entityContextBuilder.Build();

            List<DTOResultExercise> resultExercisesToRemove = new List<DTOResultExercise>();

            foreach (ResultExercise resultExercise in entityContextBuilder.TestResultExercises)
            {
                context.Entry(resultExercise).State = EntityState.Detached;

                DTOResultExercise dtoResultExercise = resultExercise.ToDTOResultExercise();

                resultExercisesToRemove.Add(dtoResultExercise);
            }

            // Act
            await context.RemoveEntities(resultExercisesToRemove, isNeedSaveChanges);

            foreach (DTOResultExercise removedDTOResultExercise in resultExercisesToRemove)
            {
                ResultExercise? removedResultExercise = context.ResultsExercises.Find(removedDTOResultExercise.Id);

                // Assert
                if (isNeedSaveChanges)
                    removedResultExercise.Should().BeNull();
                else
                    removedResultExercise.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetDomainByDTO_WithDTO_ShouldFindCycleDomain()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle();

            EntityContext context = entityContextBuilder.Build();

            // Act
            IDTODomain DTOCycle = entityContextBuilder.TestCycle.ToDTOCycle();

            IDomain domain = await context.GetDomainByDTO(DTOCycle);

            // Assert
            domain.Id.Should().NotBe(0);
            domain.IsArchive.Should().BeFalse();
        }

        [Fact]
        public async Task GetDomainByDTO_WithDTO_ShouldFindDayDomain()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay();

            EntityContext context = entityContextBuilder.Build();

            // Act
            IDTODomain DTODay = entityContextBuilder.TestDay.ToDTODay();

            IDomain domain = await context.GetDomainByDTO(DTODay);

            // Assert
            domain.Id.Should().NotBe(0);
            domain.IsArchive.Should().BeFalse();
        }

        [Fact]
        public async Task GetDomainByDTO_WithDTO_ShouldFindExerciseDomain()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercise();

            EntityContext context = entityContextBuilder.Build();

            // Act
            IDTODomain DTOExercise = entityContextBuilder.TestExercise.ToDTOExercise();

            IDomain domain = await context.GetDomainByDTO(DTOExercise);

            // Assert
            domain.Id.Should().NotBe(0);
            domain.IsArchive.Should().BeFalse();
        }
    }
}