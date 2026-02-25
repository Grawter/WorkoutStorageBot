using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.BusinessLogic.Helpers.SharedBusinessLogic
{
    public class SharedExercisesAndResultsLogicHelperTests
    {
        [Fact]
        public void GetAllUserResultsExercises_WithExistiongResultExercises_ShouldReturnExpectedResults()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises()
                                                                                        .WithResultExercises();
            EntityContext entityContext = entityContextBuilder.Build();

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            // Act
            IQueryable<ResultExercise> resultsExercises = SharedExercisesAndResultsLogicHelper.GetAllUserResultsExercises(entityContext, userContext);

            // Assert
            resultsExercises.Should().HaveCount(entityContextBuilder.TestResultExercises.Count);
            resultsExercises.Select(x => x.Id).Should().BeEquivalentTo(entityContextBuilder.TestResultExercises.Select(x => x.Id));
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithCurrentDay_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 2);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentResultExercises(entityContext, entityContextBuilder.TestExercises);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);
            userContext.DataManager.SetCurrentDomain(DTOCycle.Days.Single());

            string expectedText = @"Найденная тренировка:
======================
Дата: 02.06.2020
Упражнение: ""<b>TestExercise1</b>""
Повторения: (10)
Упражнение: ""<b>TestExercise2</b>""
Повторения: (30) => Вес: (20)
Упражнение: ""<b>TestExercise3</b>""
=> 123
======================

Выберите упражнение из дня ""<b>TestDay</b>"" (<b>TestCycle</b>)
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, true);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout));
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithCurrentDay_ShouldReturnExpectedIInformationSetWithTwoSuggestions()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 3);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentResultExercises(entityContext, entityContextBuilder.TestExercises);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);
            userContext.DataManager.SetCurrentDomain(DTOCycle.Days.Single());

            string expectedText = @"Не удалось найти тренировки с датой '03.06.2020'
======================

Найдены ближайшие тренировки к искомой дате:
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, true);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            informationSet.AdditionalParameters.Should().NotBeNull();
            informationSet.AdditionalParameters.Should().HaveCount(3);
            informationSet.AdditionalParameters["domainTypeForFind"].Should().Be("Exercise");
            informationSet.AdditionalParameters["trainingDateLessThanFinderDate"].Should().Be("02.06.2020");
            informationSet.AdditionalParameters["trainingDateGreaterThanFinderDate"].Should().Be("04.06.2020");
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithCurrentDay_ShouldReturnExpectedIInformationSetWithOneSuggestionWithLessDate()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 5);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentResultExercises(entityContext, entityContextBuilder.TestExercises);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);
            userContext.DataManager.SetCurrentDomain(DTOCycle.Days.Single());

            string expectedText = @"Не удалось найти тренировки с датой '05.06.2020'
======================

Найдены ближайшие тренировки к искомой дате:
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, true);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            informationSet.AdditionalParameters.Should().NotBeNull();
            informationSet.AdditionalParameters.Should().HaveCount(2);
            informationSet.AdditionalParameters["domainTypeForFind"].Should().Be("Exercise");
            informationSet.AdditionalParameters["trainingDateLessThanFinderDate"].Should().Be("04.06.2020");
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithCurrentDay_ShouldReturnExpectedIInformationSetWithOneSuggestionWithGreaterDate()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 1);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle()
                                                                                        .WithDay()
                                                                                        .WithExercises();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentResultExercises(entityContext, entityContextBuilder.TestExercises);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);
            userContext.DataManager.SetCurrentDomain(DTOCycle.Days.Single());

            string expectedText = @"Не удалось найти тренировки с датой '01.06.2020'
======================

Найдены ближайшие тренировки к искомой дате:
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, true);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.ExercisesListWithLastWorkoutForDay));
            informationSet.AdditionalParameters.Should().NotBeNull();
            informationSet.AdditionalParameters.Should().HaveCount(2);
            informationSet.AdditionalParameters["domainTypeForFind"].Should().Be("Exercise");
            informationSet.AdditionalParameters["trainingDateGreaterThanFinderDate"].Should().Be("02.06.2020");
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithDifferentDays_ShouldReturnExpectedIInformationSet()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 2);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle(isActive: true)
                                                                                        .WithDays();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentExercisesAndResultExercises(entityContext, entityContextBuilder.TestDays);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);

            string expectedText = @"Найденная тренировка:
======================
Дата: 02.06.2020
Упражнение: ""<b>Exercise1</b>""
Повторения: (22)
Упражнение: ""<b>Exercise2</b>""
Повторения: (10)
Упражнение: ""<b>Exercise3</b>""
Повторения: (30) => Вес: (20)
Упражнение: ""<b>Exercise4</b>""
=> 123
======================

Выберите тренировочный день из цикла ""<b>TestCycle</b>""
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, false);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main));
            informationSet.AdditionalParameters.Should().BeNull();
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithDifferentDays_ShouldReturnExpectedIInformationSetWithTwoSuggestions()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 3);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle(isActive: true)
                                                                                        .WithDays();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentExercisesAndResultExercises(entityContext, entityContextBuilder.TestDays);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);

            string expectedText = @"Не удалось найти тренировки с датой '03.06.2020'
======================

Найдены ближайшие тренировки к искомой дате:
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, false);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.DaysListWithLastWorkout));
            informationSet.AdditionalParameters.Should().NotBeNull();
            informationSet.AdditionalParameters.Should().HaveCount(3);
            informationSet.AdditionalParameters["domainTypeForFind"].Should().Be("Day");
            informationSet.AdditionalParameters["trainingDateLessThanFinderDate"].Should().Be("02.06.2020");
            informationSet.AdditionalParameters["trainingDateGreaterThanFinderDate"].Should().Be("04.06.2020");
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithDifferentDays_ShouldReturnExpectedIInformationSetWithOneSuggestionWithLessDate()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 5);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle(isActive: true)
                                                                                        .WithDays();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentExercisesAndResultExercises(entityContext, entityContextBuilder.TestDays);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);

            string expectedText = @"Не удалось найти тренировки с датой '05.06.2020'
======================

Найдены ближайшие тренировки к искомой дате:
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, false);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.DaysListWithLastWorkout));
            informationSet.AdditionalParameters.Should().NotBeNull();
            informationSet.AdditionalParameters.Should().HaveCount(2);
            informationSet.AdditionalParameters["domainTypeForFind"].Should().Be("Day");
            informationSet.AdditionalParameters["trainingDateLessThanFinderDate"].Should().Be("04.06.2020");
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithDifferentDays_ShouldReturnExpectedIInformationSetWithOneSuggestionWithGreaterDate()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 1);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle(isActive: true)
                                                                                        .WithDays();

            EntityContext entityContext = entityContextBuilder.Build();

            AddDifferentExercisesAndResultExercises(entityContext, entityContextBuilder.TestDays);

            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);

            string expectedText = @"Не удалось найти тренировки с датой '01.06.2020'
======================

Найдены ближайшие тренировки к искомой дате:
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, false);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.DaysListWithLastWorkout));
            informationSet.AdditionalParameters.Should().NotBeNull();
            informationSet.AdditionalParameters.Should().HaveCount(2);
            informationSet.AdditionalParameters["domainTypeForFind"].Should().Be("Day");
            informationSet.AdditionalParameters["trainingDateGreaterThanFinderDate"].Should().Be("02.06.2020");
        }

        [Fact]
        public async Task GetInformationSetWithTextResultsByDateCommand_WithDifferentDays_ShouldReturnExpectedIInformationSetWithoutSuggestions()
        {
            // Arrange
            DateTime finderDate = new DateTime(2020, 6, 1);

            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder().Create()
                                                                                        .WithUserInformation()
                                                                                        .WithCycle(isActive: true)
                                                                                        .WithDays();

            EntityContext entityContext = entityContextBuilder.Build();


            DTOUserInformation DTOUserInformation = entityContextBuilder.TestUserInformation.ToDTOUserInformation();

            UserContext userContext = new UserContext(DTOUserInformation);

            DTOCycle DTOCycle = DTOUserInformation.Cycles.Single();

            userContext.DataManager.SetCurrentDomain(DTOCycle);

            string expectedText = @"Не удалось найти тренировки с датой '01.06.2020'
======================

Не удалось найти ближайших тренировок к искомой дате
";

            // Act
            IInformationSet informationSet =
                await SharedExercisesAndResultsLogicHelper.GetInformationSetWithTextResultsByDateCommand(entityContext, userContext, finderDate, false);

            // Assert
            informationSet.Message.Should().Be(expectedText);
            informationSet.ButtonsSets.Should().Be((ButtonsSet.FoundResultsByDate, ButtonsSet.DaysListWithLastWorkout));
            informationSet.AdditionalParameters.Should().NotBeNull();
            informationSet.AdditionalParameters.Should().HaveCount(1);
            informationSet.AdditionalParameters["domainTypeForFind"].Should().Be("Day");
        }

        private static void AddDifferentResultExercises(EntityContext entityContext, List<Exercise> testExercises)
        {
            ResultExercise resultExercise1 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 2),
                ExerciseId = testExercises[0].Id,
                Count = 10
            };

            ResultExercise resultExercise2 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 2),
                ExerciseId = testExercises[1].Id,
                Weight = 20,
                Count = 30
            };

            ResultExercise resultExercise3 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 2),
                ExerciseId = testExercises[2].Id,
                FreeResult = "123"
            };

            ResultExercise resultExercise4 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 4),
                ExerciseId = testExercises[2].Id,
                FreeResult = "321"
            };

            entityContext.ResultsExercises.AddRange([resultExercise1, resultExercise2, resultExercise3, resultExercise4]);
            entityContext.SaveChanges();
        }

        private static void AddDifferentExercisesAndResultExercises(EntityContext entityContext, List<Day> testDays)
        {
            Exercise exercise1 = new Exercise()
            {
                Name = "Exercise1",
                Mode = ExercisesMods.Count,
                DayId = testDays[0].Id,
            };

            Exercise exercise2 = new Exercise()
            {
                Name = "Exercise2",
                Mode = ExercisesMods.Count,
                DayId = testDays[1].Id,
            };

            Exercise exercise3 = new Exercise()
            {
                Name = "Exercise3",
                Mode = ExercisesMods.WeightCount,
                DayId = testDays[1].Id,
            };

            Exercise exercise4 = new Exercise()
            {
                Name = "Exercise4",
                Mode = ExercisesMods.FreeResult,
                DayId = testDays[1].Id,
            };

            entityContext.Exercises.AddRange([exercise1, exercise2, exercise3, exercise4]);
            entityContext.SaveChanges();

            ResultExercise resultExercise1 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 2),
                ExerciseId = exercise1.Id,
                Count = 22
            };

            ResultExercise resultExercise2 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 2),
                ExerciseId = exercise2.Id,
                Count = 10
            };

            ResultExercise resultExercise3 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 2),
                ExerciseId = exercise3.Id,
                Weight = 20,
                Count = 30
            };

            ResultExercise resultExercise4 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 2),
                ExerciseId = exercise4.Id,
                FreeResult = "123"
            };

            ResultExercise resultExercise5 = new ResultExercise()
            {
                DateTime = new DateTime(2020, 6, 4),
                ExerciseId = exercise4.Id,
                FreeResult = "321"
            };

            entityContext.ResultsExercises.AddRange([resultExercise1, resultExercise2, resultExercise3, resultExercise4, resultExercise5]);
            entityContext.SaveChanges();
        }
    }
}