using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Exceptions
{
    public class ModelExtensionTests
    {
        [Fact]
        public void ToDTOUserInformation_WithUserInformation_ShouldMapAllPropertiesToDTO()
        {
            // Arrange
            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithUserInformation()
                                                                            .WithCycleByUserInformation()
                                                                            .WithDayByCycle()
                                                                            .WithExerciseByDay()
                                                                            .WithResultExerciseByExercise();

            UserInformation userInformation = domainModelBuilder.TestUserInformation;

            // Act
            DTOUserInformation DTOUserInformation = userInformation.ToDTOUserInformation();

            // Assert
            DTOUserInformation.Id.Should().Be(userInformation.Id);
            DTOUserInformation.UserId.Should().Be(userInformation.UserId);
            DTOUserInformation.FirstName.Should().Be(userInformation.FirstName);
            DTOUserInformation.Username.Should().Be(userInformation.Username);
            DTOUserInformation.WhiteList.Should().Be(userInformation.WhiteList);
            DTOUserInformation.BlackList.Should().Be(userInformation.BlackList);
            DTOUserInformation.Cycles.Count.Should().Be(1);
            DTOUserInformation.Cycles.Single().Days.Count.Should().Be(1);
            DTOUserInformation.Cycles.Single().Days.Single().Exercises.Count.Should().Be(1);
            DTOUserInformation.Cycles.Single().Days.Single().Exercises.Single().ResultsExercise.Count.Should().Be(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToDTOCycle_WithCycle_ShouldMapAllPropertiesToDTO(bool isNeedWithParent)
        {
            // Arrange
            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithCycle()
                                                                            .WithDayByCycle()
                                                                            .WithExerciseByDay()
                                                                            .WithResultExerciseByExercise();

            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation();

            Cycle cycle = domainModelBuilder.TestCycle;
            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;

            // Act
            DTOCycle DTOCycle = isNeedWithParent ? cycle.ToDTOCycle(DTOUserInformation) : cycle.ToDTOCycle();

            // Assert
            DTOCycle.Id.Should().Be(cycle.Id);
            DTOCycle.Name.Should().Be(cycle.Name);
            DTOCycle.UserInformationId.Should().Be(cycle.UserInformationId);
            DTOCycle.IsActive.Should().Be(cycle.IsActive);
            DTOCycle.IsArchive.Should().Be(cycle.IsArchive);

            if (isNeedWithParent)
                DTOCycle.UserInformation.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToDTODay_WithDay_ShouldMapAllPropertiesToDTO(bool isNeedWithParent)
        {
            // Arrange
            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithDay()
                                                                            .WithExerciseByDay()
                                                                            .WithResultExerciseByExercise();

            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle();

            Day day = domainModelBuilder.TestDay;
            DTOCycle DTOCycle = DTOModelBuilder.DTOTestCycle;
            
            // Act
            DTODay DTODay = isNeedWithParent ? day.ToDTODay(DTOCycle) : day.ToDTODay();

            // Assert
            DTODay.Id.Should().Be(day.Id);
            DTODay.Name.Should().Be(day.Name);
            DTODay.CycleId.Should().Be(day.CycleId);
            DTODay.IsArchive.Should().Be(day.IsArchive);

            if (isNeedWithParent)
                DTODay.Cycle.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToDTOExercise_WithExercise_ShouldMapAllPropertiesToDTO(bool isNeedWithParent)
        {
            // Arrange
            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithExercise()
                                                                            .WithResultExerciseByExercise();

            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay();

            Exercise exercise = domainModelBuilder.TestExercise;
            DTODay DTODay = DTOModelBuilder.DTOTestDay;

            // Act
            DTOExercise DTOExercise = isNeedWithParent ? exercise.ToDTOExercise(DTODay) : exercise.ToDTOExercise();

            // Assert
            DTOExercise.Id.Should().Be(exercise.Id);
            DTOExercise.Name.Should().Be(exercise.Name);
            DTOExercise.Mode.Should().Be(exercise.Mode);
            DTOExercise.DayId.Should().Be(exercise.DayId);
            DTOExercise.IsArchive.Should().Be(exercise.IsArchive);

            if (isNeedWithParent)
                DTOExercise.Day.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToDTOResultExercise_WithResultExercise_ShouldMapAllPropertiesToDTO(bool isNeedWithParent)
        {
            // Arrange
            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithResultExercise();

            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercise();

            ResultExercise resultExercise = domainModelBuilder.TestResultExercise;
            DTOExercise DTOExercise = DTOModelBuilder.DTOTestExercise;

            // Act
            DTOResultExercise DTOResultExercise = isNeedWithParent ? resultExercise.ToDTOResultExercise(DTOExercise) : resultExercise.ToDTOResultExercise();

            // Assert
            DTOResultExercise.Id.Should().Be(resultExercise.Id);
            DTOResultExercise.Count.Should().Be(resultExercise.Count);
            DTOResultExercise.Weight.Should().Be(resultExercise.Weight);
            DTOResultExercise.DateTime.Should().Be(resultExercise.DateTime);
            DTOResultExercise.FreeResult.Should().Be(resultExercise.FreeResult);
            DTOResultExercise.ExerciseId.Should().Be(resultExercise.ExerciseId);

            if (isNeedWithParent)
                DTOResultExercise.Exercise.Should().NotBeNull();
        }

        [Fact]
        public void ToUserInformation_WithDTOUserInformation_ShouldMapAllPropertiesToEntity()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation()
                                                                   .WithDTOCycleByDTOUserInformation()
                                                                   .WithDTODayByDTOCycle()
                                                                   .WithDTOExerciseByDTODay()
                                                                   .WithDTOResultExerciseByDTOExercise();

            DTOUserInformation DTOUserInformation = DTOModelBuilder.DTOTestUserInformation;

            // Act
            UserInformation userInformation = DTOUserInformation.ToUserInformation();

            // Assert
            userInformation.Id.Should().Be(DTOUserInformation.Id);
            userInformation.UserId.Should().Be(DTOUserInformation.UserId);
            userInformation.FirstName.Should().Be(DTOUserInformation.FirstName);
            userInformation.Username.Should().Be(DTOUserInformation.Username);
            userInformation.WhiteList.Should().Be(DTOUserInformation.WhiteList);
            userInformation.BlackList.Should().Be(DTOUserInformation.BlackList);
            userInformation.Cycles.Count.Should().Be(1);
            userInformation.Cycles.Single().Days.Count.Should().Be(1);
            userInformation.Cycles.Single().Days.Single().Exercises.Count.Should().Be(1);
            userInformation.Cycles.Single().Days.Single().Exercises.Single().ResultsExercise.Count.Should().Be(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToCycle_WithDTOCycle_ShouldMapAllPropertiesToEntity(bool isNeedWithParent)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOCycle()
                                                                   .WithDTODayByDTOCycle()
                                                                   .WithDTOExerciseByDTODay()
                                                                   .WithDTOResultExerciseByDTOExercise();

            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithUserInformation();

            DTOCycle DTOCycle = DTOModelBuilder.DTOTestCycle;
            UserInformation userInformation = domainModelBuilder.TestUserInformation;

            // Act
            Cycle cycle = isNeedWithParent ? DTOCycle.ToCycle(userInformation) : DTOCycle.ToCycle();

            // Assert
            cycle.Id.Should().Be(DTOCycle.Id);
            cycle.Name.Should().Be(DTOCycle.Name);
            cycle.UserInformationId.Should().Be(DTOCycle.UserInformationId);
            cycle.IsActive.Should().Be(DTOCycle.IsActive);
            cycle.IsArchive.Should().Be(DTOCycle.IsArchive);

            if (isNeedWithParent)
                cycle.UserInformation.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToDay_WithDTODay_ShouldMapAllPropertiesToEntity(bool isNeedWithParent)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTODay()
                                                                   .WithDTOExerciseByDTODay()
                                                                   .WithDTOResultExerciseByDTOExercise();

            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithCycle();

            DTODay DTODay = DTOModelBuilder.DTOTestDay;
            Cycle cycle = domainModelBuilder.TestCycle;
            
            // Act
            Day Day = isNeedWithParent ? DTODay.ToDay(cycle) : DTODay.ToDay();

            // Assert
            Day.Id.Should().Be(DTODay.Id);
            Day.Name.Should().Be(DTODay.Name);
            Day.CycleId.Should().Be(DTODay.CycleId);
            Day.IsArchive.Should().Be(DTODay.IsArchive);

            if (isNeedWithParent)
                Day.Cycle.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToExercise_WithDTOExercise_ShouldMapAllPropertiesToEntity(bool isNeedWithParent)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOExercise()
                                                                   .WithDTOResultExerciseByDTOExercise();

            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithDay();

            DTOExercise DTOExercise = DTOModelBuilder.DTOTestExercise;
            Day day = domainModelBuilder.TestDay;

            // Act
            Exercise exercise = isNeedWithParent ? DTOExercise.ToExercise(day) : DTOExercise.ToExercise();

            // Assert
            exercise.Id.Should().Be(DTOExercise.Id);
            exercise.Name.Should().Be(DTOExercise.Name);
            exercise.Mode.Should().Be(DTOExercise.Mode);
            exercise.DayId.Should().Be(DTOExercise.DayId);
            exercise.IsArchive.Should().Be(DTOExercise.IsArchive);

            if (isNeedWithParent)
                exercise.Day.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToResultExercise_WithDTOResultExercise_ShouldMapAllPropertiesToEntity(bool isNeedWithParent)
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOResultExercise();

            DomainModelBuilder domainModelBuilder = new DomainModelBuilder().WithExercise();

            DTOResultExercise DTOResultExercise = DTOModelBuilder.DTOTestResultExercise;
            Exercise exercise = domainModelBuilder.TestExercise;

            // Act
            ResultExercise resultExercise = isNeedWithParent ? DTOResultExercise.ToResultExercise(exercise) : DTOResultExercise.ToResultExercise();

            // Assert
            resultExercise.Id.Should().Be(DTOResultExercise.Id);
            resultExercise.Count.Should().Be(DTOResultExercise.Count);
            resultExercise.Weight.Should().Be(DTOResultExercise.Weight);
            resultExercise.DateTime.Should().Be(DTOResultExercise.DateTime);
            resultExercise.FreeResult.Should().Be(DTOResultExercise.FreeResult);
            resultExercise.ExerciseId.Should().Be(DTOResultExercise.ExerciseId);

            if (isNeedWithParent)
                resultExercise.Exercise.Should().NotBeNull();
        }
    }
}