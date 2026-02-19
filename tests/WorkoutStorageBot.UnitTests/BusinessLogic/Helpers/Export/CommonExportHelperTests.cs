using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Helpers.Export;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Export
{
    public class CommonExportHelperTests
    {
        [Fact]
        public void GetFilterDateTime_WithResultExercises_ShouldMaxDateSubtractSpecifiedMonth()
        {
            // Arrange
            IQueryable<ResultExercise> results = new List<ResultExercise>()
            {
                new ResultExercise(){ Id = 1, Count = 1, DateTime = new DateTime(2020, 6, 1) },
                new ResultExercise(){ Id = 2, Count = 2, DateTime = new DateTime(2020, 8, 1) },
                new ResultExercise(){ Id = 3, Count = 3, DateTime = new DateTime(2020, 9, 1) },
            }.AsQueryable();

            // Act
            DateTime filterDate = CommonExportHelper.GetFilterDateTime(2, results);

            // Assert
            filterDate.Should().Be(new DateTime(2020, 7, 1));
        }

        [Fact]
        public void GetResultExercisesByFilterDate_WithResultExercises_ShouldReturnResultsDateTimeMoreOrEqualFilterDate()
        {
            // Arrange
            DateTime filteredDateTime = new DateTime(2020, 8, 1);

            IQueryable<ResultExercise> results = new List<ResultExercise>()
            {
                new ResultExercise(){ Id = 1, Count = 1, DateTime = new DateTime(2020, 6, 1) },
                new ResultExercise(){ Id = 2, Count = 2, DateTime = new DateTime(2020, 8, 1) },
                new ResultExercise(){ Id = 3, Count = 3, DateTime = new DateTime(2020, 9, 1) },
            }.AsQueryable();

            // Act
            IQueryable<ResultExercise> filteredResults = CommonExportHelper.GetResultExercisesByFilterDate(results, filteredDateTime);

            // Assert
            filteredResults.Should().OnlyContain(x => x.DateTime >= filteredDateTime);
        }
    }
}