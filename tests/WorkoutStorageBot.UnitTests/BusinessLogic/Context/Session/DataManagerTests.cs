using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Context.Session
{
    public class DataManagerTests
    {
        [Fact]
        public void CreateAndSetCurrentCycle_WithoutCurrentCycle_ShouldCreateAndSetCurrentCycle()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            DataManager dataManager = new DataManager();

            // Act
            DTOCycle newCurrentCycle = dataManager.CreateAndSetCurrentCycle("TestCycle", true, DTOUserInformation);

            // Assert
            dataManager.CurrentCycle.Should().NotBeNull();
            dataManager.CurrentCycle.Name.Should().Be(newCurrentCycle.Name);
            dataManager.CurrentCycle.IsActive.Should().Be(newCurrentCycle.IsActive);
            dataManager.CurrentCycle.IsArchive.Should().Be(newCurrentCycle.IsArchive);
            dataManager.CurrentCycle.UserInformation.Should().Be(newCurrentCycle.UserInformation);
            dataManager.CurrentCycle.UserInformationId.Should().Be(newCurrentCycle.UserInformationId);
        }

        [Fact]
        public void CreateAndSetCurrentCycle_WithExistingCurrentCycle_ShouldCreateAndSetCurrentCycle()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            DataManager dataManager = new DataManager();

            DTOCycle existingDTOCycle = dataManager.CreateAndSetCurrentCycle("TestCycle", false, DTOUserInformation);

            // Act
            DTOCycle newCurrentCycle = dataManager.CreateAndSetCurrentCycle("TestCycle2", true, DTOUserInformation);

            // Assert
            dataManager.CurrentCycle.Should().NotBeNull();
            dataManager.CurrentCycle.Name.Should().Be(newCurrentCycle.Name);
            dataManager.CurrentCycle.IsActive.Should().Be(newCurrentCycle.IsActive);
            dataManager.CurrentCycle.IsArchive.Should().Be(newCurrentCycle.IsArchive);
            dataManager.CurrentCycle.UserInformation.Should().Be(newCurrentCycle.UserInformation);
            dataManager.CurrentCycle.UserInformationId.Should().Be(newCurrentCycle.UserInformationId);
        }

        [Fact]
        public void CreateAndSetCurrentDay_WithoutCurrentDay_ShouldCreateAndSetCurrentDay()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            DataManager dataManager = new DataManager();

            dataManager.CreateAndSetCurrentCycle("TestCycle", true, DTOUserInformation);

            // Act
            DTODay newCurrentDay = dataManager.CreateAndSetCurrentDay("TestDay");

            // Assert
            dataManager.CurrentDay.Should().NotBeNull();
            dataManager.CurrentDay.Name.Should().Be(newCurrentDay.Name);
            dataManager.CurrentDay.IsArchive.Should().Be(newCurrentDay.IsArchive);
            dataManager.CurrentDay.Cycle.Should().Be(newCurrentDay.Cycle);
            dataManager.CurrentDay.CycleId.Should().Be(newCurrentDay.CycleId);
        }

        [Fact]
        public void CreateAndSetCurrentDay_WithExistingCurrentDay_ShouldCreateAndSetCurrentDay()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;
            DataManager dataManager = new DataManager();

            dataManager.CreateAndSetCurrentCycle("TestCycle", true, DTOUserInformation);
            DTODay existingDTODay = dataManager.CreateAndSetCurrentDay("TestDay");

            // Act
            DTODay newCurrentDTODay = dataManager.CreateAndSetCurrentDay("TestDay2");

            // Assert
            dataManager.CurrentDay.Should().NotBeNull();
            dataManager.CurrentDay.Name.Should().Be(newCurrentDTODay.Name);
            dataManager.CurrentDay.IsArchive.Should().Be(newCurrentDTODay.IsArchive);
            dataManager.CurrentDay.Cycle.Should().Be(newCurrentDTODay.Cycle);
            dataManager.CurrentDay.CycleId.Should().Be(newCurrentDTODay.CycleId);
        }

        [Fact]
        public void CreateAndSetCurrentDay_WithoutCurrentCycle_ShouldThrowArgumentNullException()
        {
            // Arrange
            DataManager dataManager = new DataManager();

            // Act
            Action act = () => dataManager.CreateAndSetCurrentDay("TestDay");

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetCurrentDateTimeToExerciseTimer_ShouldExerciseTimerHasValue()
        {
            // Arrange
            DataManager dataManager = new DataManager();

            // Act
            dataManager.SetCurrentDateTimeToExerciseTimer();

            // Assert
            dataManager.ExerciseTimer.Should().NotBe(DateTime.MinValue);
        }

        [Fact]
        public void ResetExerciseTimer_ShouldExerciseTimerHasntValue()
        {
            // Arrange
            DataManager dataManager = new DataManager();
            dataManager.SetCurrentDateTimeToExerciseTimer();

            // Act
            dataManager.ResetExerciseTimer();

            // Assert
            dataManager.ExerciseTimer.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void ResetExerciseTimer_WithExistingExerciseTimer_ShouldExerciseTimerHasntValue()
        {
            // Arrange
            DataManager dataManager = new DataManager();
            dataManager.SetCurrentDateTimeToExerciseTimer();

            // Act
            dataManager.ResetExerciseTimer();

            // Assert
            dataManager.ExerciseTimer.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void TryAddTempExercises_WithEmptyCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            DataManager dataManager = new DataManager();

            List<DTOExercise> exercises = new List<DTOExercise>();

            // Act
            Action act = () => dataManager.TryAddTempExercises(exercises, out string existingExerciseName);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void TryAddTempExercises_WithoutCurrentDay_ShouldThrowArgumentNullException()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercises();

            DataManager dataManager = new DataManager();

            List<DTOExercise> exercises = DTOModelBuilder.DTOTestExercises;

            // Act
            Action act = () => dataManager.TryAddTempExercises(exercises, out string existingExerciseName);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TryAddTempExercises_WithCurrentDay_ShouldAddedTempExercises()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay()
                                                                   .WithDTOExercises();
            DataManager dataManager = new DataManager();

            DTODay currentDay = DTOModelBuilder.DTOTestDay;

            dataManager.SetCurrentDomain(currentDay);

            List<DTOExercise> tempExercises = DTOModelBuilder.DTOTestExercises;

            // Act
            bool result = dataManager.TryAddTempExercises(tempExercises, out string existingExerciseName);

            // Assert
            result.Should().BeTrue();
            dataManager.TempExercises.Should().HaveCount(tempExercises.Count);
            dataManager.TempExercises.Should().BeEquivalentTo(tempExercises);
        }

        [Fact]
        public void TryAddTempExercises_WithCurrentDayAndExistingTempExercises_ShouldAddedTempExercises()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay()
                                                                   .WithDTOExercises();

            DataManager dataManager = new DataManager();

            DTODay DTODay = DTOModelBuilder.DTOTestDay;

            dataManager.SetCurrentDomain(DTODay);

            List<DTOExercise> existingExercises = DTOModelBuilder.DTOTestExercises;

            dataManager.TryAddTempExercises(existingExercises, out string _);

            List<DTOExercise> newExercises = new List<DTOExercise>()
            {
                new DTOExercise() { Mode = ExercisesMods.Count, Name = "TestExercise5" },
                new DTOExercise() { Mode = ExercisesMods.WeightCount, Name = "TestExercise6" },
                new DTOExercise() { Mode = ExercisesMods.Timer, Name = "TestExercise7" },
                new DTOExercise() { Mode = ExercisesMods.FreeResult, Name = "TestExercise8" }
            };

            // Act
            bool result = dataManager.TryAddTempExercises(newExercises, out string existingExerciseName);

            // Assert
            result.Should().BeTrue();
            dataManager.TempExercises.Should().HaveCount(existingExercises.Count + newExercises.Count);
        }

        [Fact]
        public void TryAddTempExercises_WithCurrentDay_ShouldNotAddedTempExercisesWithNonOriginalName()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay();
            DataManager dataManager = new DataManager();

            DTODay DTODay = DTOModelBuilder.DTOTestDay;

            dataManager.SetCurrentDomain(DTODay);

            List<DTOExercise> newExercises = new List<DTOExercise>()
            {
                new DTOExercise() { Mode = ExercisesMods.Count, Name = "TestExercise1" },
                new DTOExercise() { Mode = ExercisesMods.WeightCount, Name = "TestExercise2" },
                new DTOExercise() { Mode = ExercisesMods.Timer, Name = "TestExercise3" },
                new DTOExercise() { Mode = ExercisesMods.FreeResult, Name = "TestExercise3" }
            };

            // Act
            bool result = dataManager.TryAddTempExercises(newExercises, out string existingExerciseName);

            // Assert
            result.Should().BeFalse();
            dataManager.TempExercises.Should().HaveCount(0);
            existingExerciseName.Should().Be("TestExercise3");
        }

        [Fact]
        public void TryAddTempExercises_WithCurrentDayAndExistingTempExercises_ShouldNotAddedTempExerciseWithNonOriginalName()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay()
                                                                   .WithDTOExercises();
            DataManager dataManager = new DataManager();

            DTODay DTODay = DTOModelBuilder.DTOTestDay;

            dataManager.SetCurrentDomain(DTODay);

            List<DTOExercise> existingExercises = DTOModelBuilder.DTOTestExercises;

            dataManager.TryAddTempExercises(existingExercises, out string _);

            List<DTOExercise> newExercises = new List<DTOExercise>()
            {
                new DTOExercise() { Mode = ExercisesMods.Count, Name = "TestExercise5" },
                new DTOExercise() { Mode = ExercisesMods.WeightCount, Name = "TestExercise6" },
                new DTOExercise() { Mode = ExercisesMods.Timer, Name = "TestExercise7" },
                new DTOExercise() { Mode = ExercisesMods.FreeResult, Name = "TestExercise7" }
            };

            // Act
            bool result = dataManager.TryAddTempExercises(newExercises, out string existingExerciseName);

            // Assert
            result.Should().BeFalse();
            dataManager.TempExercises.Should().HaveCount(existingExercises.Count);
            dataManager.TempExercises.Should().BeEquivalentTo(existingExercises);
            existingExerciseName.Should().Be("TestExercise7");
        }

        [Fact]
        public void ResetTempExercises_WithCurrentDayAndExistingTempExercises_ShouldResetExistingTempExercises()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay()
                                                                   .WithDTOExercises();
            DataManager dataManager = new DataManager();

            DTODay DTODay = DTOModelBuilder.DTOTestDay;

            dataManager.SetCurrentDomain(DTODay);

            List<DTOExercise> existingExercises = DTOModelBuilder.DTOTestExercises;

            dataManager.TryAddTempExercises(existingExercises, out string _);

            // Act
            dataManager.ResetTempExercises();

            // Assert
            dataManager.TempExercises.Should().BeNull();
        }

        [Fact]
        public void AddTempResultsExercise_WithEmptyCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            DataManager dataManager = new DataManager();

            List<DTOResultExercise> results = new List<DTOResultExercise>();

            // Act
            Action act = () => dataManager.AddTempResultsExercise(results);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AddTempResultsExercise_WithoutCurrentExercise_ShouldThrowArgumentNullException()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOResultExercises();
            DataManager dataManager = new DataManager();

            List<DTOResultExercise> results = DTOModelBuilder.DTOTestResultExercises;

            // Act
            Action act = () => dataManager.AddTempResultsExercise(results);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddTempResultsExercise_WithCurrentExercise_ShouldAddedTempResultsExercise()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercise()
                                                                   .WithDTOResultExercises();
            DTOExercise DTOExercise = DTOModelBuilder.DTOTestExercise;
            DataManager dataManager = new DataManager();

            dataManager.SetCurrentDomain(DTOExercise);

            List<DTOResultExercise> results = DTOModelBuilder.DTOTestResultExercises;

            // Act
            dataManager.AddTempResultsExercise(results);

            // Assert
            dataManager.TempResultsExercise.Should().HaveCount(results.Count);
            dataManager.TempResultsExercise.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void AddTempResultsExercise_WithCurrentExerciseAndTestResultExercises_ShouldAddedTempResultsExercise()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercise()
                                                                   .WithDTOResultExercises();
            DTOExercise DTOExercise = DTOModelBuilder.DTOTestExercise;
            DataManager dataManager = new DataManager();

            dataManager.SetCurrentDomain(DTOExercise);

            List<DTOResultExercise> existingResults = DTOModelBuilder.DTOTestResultExercises;
            dataManager.AddTempResultsExercise(existingResults);

            List<DTOResultExercise> newResults =
           [
                new DTOResultExercise() { Id = 1, Count = 10, Weight = 20, FreeResult = "DTOResultExercise1", DateTime = DateTime.Now },
                new DTOResultExercise() { Id = 2, Count = 20, Weight = 40, FreeResult = "DTOResultExercise2", DateTime = DateTime.Now.AddMinutes(1) },
                new DTOResultExercise() { Id = 3, Count = 30, Weight = 60, FreeResult = "DTOResultExercise3", DateTime = DateTime.Now.AddMinutes(2) },
                new DTOResultExercise() { Id = 4, Count = 40, Weight = 80, FreeResult = "DTOResultExercise4", DateTime = DateTime.Now.AddMinutes(3) },
            ];

            // Act
            dataManager.AddTempResultsExercise(newResults);

            // Assert
            dataManager.TempResultsExercise.Should().HaveCount(existingResults.Count + newResults.Count);
        }

        [Fact]
        public void ResetTempResultsExercise_WithCurrentExerciseExistingTestResultExercises_ShouldResetTempResultsExercise()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercise()
                                                                   .WithDTOResultExercises();
            DTOExercise DTOExercise = DTOModelBuilder.DTOTestExercise;
            DataManager dataManager = new DataManager();

            dataManager.SetCurrentDomain(DTOExercise);

            List<DTOResultExercise> results = DTOModelBuilder.DTOTestResultExercises;
            dataManager.AddTempResultsExercise(results);

            // Act
            dataManager.ResetTempResultsExercise();

            // Assert
            dataManager.TempResultsExercise.Should().BeNull();
        }

        [Fact]
        public void ResetAll_WithAllCurrentData_ShouldResetAllCurrentData()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle()
                                                                   .WithDTODay()
                                                                   .WithDTOExercise()
                                                                   .WithDTOExercises()
                                                                   .WithDTOResultExercise()
                                                                   .WithDTOResultExercises();

            DataManager dataManager = new DataManager();

            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestCycle);
            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestDay);
            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestExercise);

            dataManager.TryAddTempExercises(DTOModelBuilder.DTOTestExercises, out string _);
            dataManager.AddTempResultsExercise(DTOModelBuilder.DTOTestResultExercises);

            dataManager.SetCurrentDateTimeToExerciseTimer();

            // Act
            dataManager.ResetAll();

            // Assert
            dataManager.CurrentCycle.Should().BeNull();
            dataManager.CurrentDay.Should().BeNull();
            dataManager.CurrentExercise.Should().BeNull();

            dataManager.TempExercises.Should().BeNull();
            dataManager.TempResultsExercise.Should().BeNull();

            dataManager.ExerciseTimer.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void ResetAll_WithCurrentCycle_ShouldResetCurrentCycle()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle();

            DataManager dataManager = new DataManager();

            DTOCycle currentCycle = DTOModelBuilder.DTOTestCycle;

            dataManager.SetCurrentDomain(currentCycle);

            // Act
            dataManager.ResetCurrentDomain(currentCycle);

            // Assert
            dataManager.CurrentCycle.Should().BeNull();
        }

        [Fact]
        public void ResetAll_WithCurrentDay_ShouldResetCurrentDay()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay();

            DataManager dataManager = new DataManager();

            DTODay currentDay = DTOModelBuilder.DTOTestDay;

            dataManager.SetCurrentDomain(currentDay);

            // Act
            dataManager.ResetCurrentDomain(currentDay);

            // Assert
            dataManager.CurrentDay.Should().BeNull();
        }

        [Fact]
        public void ResetAll_WithCurrentExercise_ShouldResetCurrentExercise()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercise();

            DataManager dataManager = new DataManager();

            DTOExercise currentExercise = DTOModelBuilder.DTOTestExercise;

            dataManager.SetCurrentDomain(currentExercise);

            // Act
            dataManager.ResetCurrentDomain(currentExercise);

            // Assert
            dataManager.CurrentExercise.Should().BeNull();
        }

        [Fact]
        public void SetCurrentDomain_WithCycle_ShouldSetCurrentCycle()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle();

            DataManager dataManager = new DataManager();

            DTOCycle currentCycle = DTOModelBuilder.DTOTestCycle;

            // Act
            dataManager.SetCurrentDomain(currentCycle);

            // Assert
            dataManager.CurrentCycle.Should().Be(currentCycle);
        }

        [Fact]
        public void SetCurrentDomain_WithCycleAndOtherCurrentData_ShouldSetCurrentCycleAndResetOtherCurrentData()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle()
                                                                   .WithDTODay()
                                                                   .WithDTOExercise()
                                                                   .WithDTOExercises()
                                                                   .WithDTOResultExercise()
                                                                   .WithDTOResultExercises();

            DataManager dataManager = new DataManager();

            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestDay);
            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestExercise);

            dataManager.TryAddTempExercises(DTOModelBuilder.DTOTestExercises, out string _);
            dataManager.AddTempResultsExercise(DTOModelBuilder.DTOTestResultExercises);

            dataManager.SetCurrentDateTimeToExerciseTimer();


            DTOCycle currentCycle = DTOModelBuilder.DTOTestCycle;

            // Act
            dataManager.SetCurrentDomain(currentCycle);

            // Assert
            dataManager.CurrentCycle.Should().Be(currentCycle);
            dataManager.CurrentDay.Should().BeNull();
            dataManager.CurrentExercise.Should().BeNull();

            dataManager.TempExercises.Should().BeNull();
            dataManager.TempResultsExercise.Should().BeNull();

            dataManager.ExerciseTimer.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void SetCurrentDomain_WithDay_ShouldSetCurrentDay()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay();

            DataManager dataManager = new DataManager();

            DTODay currentDay = DTOModelBuilder.DTOTestDay;

            // Act
            dataManager.SetCurrentDomain(currentDay);

            // Assert
            dataManager.CurrentDay.Should().Be(currentDay);
        }

        [Fact]
        public void SetCurrentDomain_WithDayAndOtherCurrentData_ShouldSetCurrentDayAndResetOtherCurrentData()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay()
                                                                   .WithDTOExercise()
                                                                   .WithDTOExercises()
                                                                   .WithDTOResultExercise()
                                                                   .WithDTOResultExercises();

            DataManager dataManager = new DataManager();

            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestDay);
            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestExercise);

            dataManager.TryAddTempExercises(DTOModelBuilder.DTOTestExercises, out string _);
            dataManager.AddTempResultsExercise(DTOModelBuilder.DTOTestResultExercises);

            dataManager.SetCurrentDateTimeToExerciseTimer();


            DTODay currentDay = new DTODay() { Name = "TestDay2" };

            // Act
            dataManager.SetCurrentDomain(currentDay);

            // Assert
            dataManager.CurrentDay.Should().Be(currentDay);
            dataManager.CurrentExercise.Should().BeNull();

            dataManager.TempExercises.Should().BeNull();
            dataManager.TempResultsExercise.Should().BeNull();

            dataManager.ExerciseTimer.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void SetCurrentDomain_WithExercise_ShouldSetCurrentExercise()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercise();

            DataManager dataManager = new DataManager();

            DTOExercise currentExercise = DTOModelBuilder.DTOTestExercise;

            // Act
            dataManager.SetCurrentDomain(currentExercise);

            // Assert
            dataManager.CurrentExercise.Should().Be(currentExercise);
        }

        [Fact]
        public void SetCurrentDomain_WithExerciseAndOtherCurrentData_ShouldSetCurrentExerciseAndResetOtherCurrentData()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay()
                                                                   .WithDTOExercise()
                                                                   .WithDTOExercises()
                                                                   .WithDTOResultExercise()
                                                                   .WithDTOResultExercises();

            DataManager dataManager = new DataManager();

            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestDay);
            dataManager.SetCurrentDomain(DTOModelBuilder.DTOTestExercise);
            dataManager.TryAddTempExercises(DTOModelBuilder.DTOTestExercises, out string _);
            dataManager.AddTempResultsExercise(DTOModelBuilder.DTOTestResultExercises);

            dataManager.SetCurrentDateTimeToExerciseTimer();

            DTOExercise currentExercise = new DTOExercise() { Name = "TestExercise2", Mode = ExercisesMods.Count };

            // Act
            dataManager.SetCurrentDomain(currentExercise);

            // Assert
            dataManager.CurrentExercise.Should().Be(currentExercise);

            dataManager.TempExercises.Should().BeNull();
            dataManager.TempResultsExercise.Should().BeNull();

            dataManager.ExerciseTimer.Should().Be(DateTime.MinValue);
        }

        [Theory]
        [InlineData(CommonConsts.DomainsAndEntities.Cycle)]
        [InlineData(CommonConsts.DomainsAndEntities.Day)]
        [InlineData(CommonConsts.DomainsAndEntities.Exercise)]
        public void GetCurrentDomain_WithoutExistingCurrentDomainAndFindedByString_ShouldReturnNull(string domainType)
        {
            // Arrange
            DataManager dataManager = new DataManager();

            // Act
            IDTODomain? domain = dataManager.GetCurrentDomain(domainType);

            // Assert
            domain.Should().BeNull();
        }

        [Theory]
        [InlineData(CommonConsts.DomainsAndEntities.Cycle)]
        [InlineData(CommonConsts.DomainsAndEntities.Day)]
        [InlineData(CommonConsts.DomainsAndEntities.Exercise)]
        public void GetRequiredCurrentDomain_WithoutExistingCurrentDomainAndFindedByString_ShouldThrowArgumentNullException(string domainType)
        {
            // Arrange
            DataManager dataManager = new DataManager();

            // Act
            Action act = () => dataManager.GetRequiredCurrentDomain(domainType);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(DomainType.Cycle)]
        [InlineData(DomainType.Day)]
        [InlineData(DomainType.Exercise)]
        public void GetCurrentDomain_WithoutExistingCurrentDomainAndFindedByEnum_ShouldReturnNull(DomainType domainType)
        {
            // Arrange
            DataManager dataManager = new DataManager();

            // Act
            IDTODomain? domain = dataManager.GetCurrentDomain(domainType);

            // Assert
            domain.Should().BeNull();
        }

        [Theory]
        [InlineData(DomainType.Cycle)]
        [InlineData(DomainType.Day)]
        [InlineData(DomainType.Exercise)]
        public void GetRequiredCurrentDomain_WithoutExistingCurrentDomainAndFindedByEnum_ShouldThrowArgumentNullException(DomainType domainType)
        {
            // Arrange
            DataManager dataManager = new DataManager();

            // Act
            Action act = () => dataManager.GetRequiredCurrentDomain(domainType);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(CommonConsts.DomainsAndEntities.Cycle)]
        [InlineData(CommonConsts.DomainsAndEntities.Day)]
        [InlineData(CommonConsts.DomainsAndEntities.Exercise)]
        public void GetCurrentDomain_WithExistingCurrentDomainAndFindedByString_ShouldReturnDomain(string domainType)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle()
                                                                   .WithDTODay()
                                                                   .WithDTOExercise();

            DataManager dataManager = new DataManager();

            IDTODomain existingDomain;

            if (domainType == CommonConsts.DomainsAndEntities.Cycle)
                existingDomain = DTOModelBuilder.DTOTestCycle;
            else if (domainType == CommonConsts.DomainsAndEntities.Day)
                existingDomain = DTOModelBuilder.DTOTestDay;
            else
                existingDomain = DTOModelBuilder.DTOTestExercise;

            dataManager.SetCurrentDomain(existingDomain);

            // Act
            IDTODomain? domain = dataManager.GetCurrentDomain(domainType);

            // Assert
            domain.Should().NotBeNull();
            domain.Should().Be(existingDomain);
        }

        [Theory]
        [InlineData(CommonConsts.DomainsAndEntities.Cycle)]
        [InlineData(CommonConsts.DomainsAndEntities.Day)]
        [InlineData(CommonConsts.DomainsAndEntities.Exercise)]
        public void GetRequiredCurrentDomain_WithExistingCurrentDomainAndFindedByString_ShouldReturnDomain(string domainType)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle()
                                                                   .WithDTODay()
                                                                   .WithDTOExercise();

            DataManager dataManager = new DataManager();

            IDTODomain existingDomain;

            if (domainType == CommonConsts.DomainsAndEntities.Cycle)
                existingDomain = DTOModelBuilder.DTOTestCycle;
            else if (domainType == CommonConsts.DomainsAndEntities.Day)
                existingDomain = DTOModelBuilder.DTOTestDay;
            else
                existingDomain = DTOModelBuilder.DTOTestExercise;

            dataManager.SetCurrentDomain(existingDomain);

            // Act
            IDTODomain domain = dataManager.GetRequiredCurrentDomain(domainType);

            // Assert
            domain.Should().NotBeNull();
            domain.Should().Be(existingDomain);
        }

        [Theory]
        [InlineData(DomainType.Cycle)]
        [InlineData(DomainType.Day)]
        [InlineData(DomainType.Exercise)]
        public void GetCurrentDomain_WithExistingCurrentDomainAndFindedByEnum_ShouldReturnDomain(DomainType domainType)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle()
                                                                   .WithDTODay()
                                                                   .WithDTOExercise();

            DataManager dataManager = new DataManager();

            IDTODomain existingDomain;

            if (domainType == DomainType.Cycle)
                existingDomain = DTOModelBuilder.DTOTestCycle;
            else if (domainType == DomainType.Day)
                existingDomain = DTOModelBuilder.DTOTestDay;
            else
                existingDomain = DTOModelBuilder.DTOTestExercise;

            dataManager.SetCurrentDomain(existingDomain);

            // Act
            IDTODomain? domain = dataManager.GetCurrentDomain(domainType);

            // Assert
            domain.Should().NotBeNull();
            domain.Should().Be(existingDomain);
        }

        [Theory]
        [InlineData(DomainType.Cycle)]
        [InlineData(DomainType.Day)]
        [InlineData(DomainType.Exercise)]
        public void GetRequiredCurrentDomain_WithExistingCurrentDomainAndFindedByEnum_ShouldReturnDomain(DomainType domainType)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle()
                                                                   .WithDTODay()
                                                                   .WithDTOExercise();

            DataManager dataManager = new DataManager();

            IDTODomain existingDomain;

            if (domainType == DomainType.Cycle)
                existingDomain = DTOModelBuilder.DTOTestCycle;
            else if (domainType == DomainType.Day)
                existingDomain = DTOModelBuilder.DTOTestDay;
            else
                existingDomain = DTOModelBuilder.DTOTestExercise;

            dataManager.SetCurrentDomain(existingDomain);

            // Act
            IDTODomain domain = dataManager.GetRequiredCurrentDomain(domainType);

            // Assert
            domain.Should().NotBeNull();
            domain.Should().Be(existingDomain);
        }
    }
}