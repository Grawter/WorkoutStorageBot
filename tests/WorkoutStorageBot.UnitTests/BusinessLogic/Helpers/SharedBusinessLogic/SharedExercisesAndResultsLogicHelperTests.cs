using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Exceptions;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.SharedBusinessLogic
{
    public class SharedExercisesAndResultsLogicHelperTests
    {
        [Fact]
        public void GetAllUserExercisesIds_ShouldReturnExpectedAllExercisesIDs()
        {
            // Arrange
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation()
                                                                   .WithDTOCycleByDTOUserInformation()
                                                                   .WithDTODayByDTOCycle();

            List<DTOExercise> DTOExercises = new List<DTOExercise>()
            {
                new DTOExercise() { Id = 1, Mode = ExercisesMods.Count, Name = "DTOExercise1" },
                new DTOExercise() { Id = 5, Mode = ExercisesMods.Count, Name = "DTOExercise2" },
                new DTOExercise() { Id = 7, Mode = ExercisesMods.Count, Name = "DTOExercise3" },
            };

            IEnumerable<int> exercisesIDs = DTOExercises.Select(x => x.Id);

            DTOModelBuilder.DTOTestDay.Exercises = DTOExercises;

            UserContext userContext = new UserContext(DTOModelBuilder.DTOTestUserInformation, Roles.User, false);

            // Act
            IEnumerable<int> allUserExercisesIDs = SharedExercisesAndResultsLogicHelper.GetAllUserExercisesIds(userContext);

            // Assert
            allUserExercisesIDs.Should().BeEquivalentTo(exercisesIDs);
        }

        [Fact]
        public void GetExercisesFromText_WithEmptyText_ShouldThrowCreateExerciseException()
        {
            // Arrange
            string text = string.Empty;

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            act.Should().Throw<CreateExerciseException>();
        }

        [Fact]
        public void GetExercisesFromText_WithTextWithoutExerciseType_ShouldThrowCreateExerciseException()
        {
            // Arrange
            string text = "Exercise0-;Exercise1-1";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            act.Should().Throw<CreateExerciseException>();
        }

        [Fact]
        public void GetExercisesFromText_WithTextWithoutExerciseName_ShouldThrowCreateExerciseException()
        {
            // Arrange
            string text = "Exercise0-0;-1";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            act.Should().Throw<CreateExerciseException>();
        }

        [Fact]
        public void GetExercisesFromText_WithOneExerciseText_ShouldReturnOneExercise()
        {
            // Arrange
            string text = "Exercise0-0";

            // Act
            List<DTOExercise> DTOExercises = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            DTOExercises.Should().HaveCount(1);

            DTOExercises[0].Name.Should().Be("Exercise0");
            DTOExercises[0].Mode.Should().Be(ExercisesMods.Count);
        }

        [Fact]
        public void GetExercisesFromText_WithSomeExercisesText_ShouldReturnSomeExercises()
        {
            // Arrange
            string text = "Exercise0-0;Exercise1-1;Exercise2-2;Exercise3-3;";

            // Act
            List<DTOExercise> DTOExercises = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            DTOExercises.Should().HaveCount(4);

            DTOExercises[0].Name.Should().Be("Exercise0");
            DTOExercises[0].Mode.Should().Be(ExercisesMods.Count);

            DTOExercises[1].Name.Should().Be("Exercise1");
            DTOExercises[1].Mode.Should().Be(ExercisesMods.WeightCount);

            DTOExercises[2].Name.Should().Be("Exercise2");
            DTOExercises[2].Mode.Should().Be(ExercisesMods.Timer);

            DTOExercises[3].Name.Should().Be("Exercise3");
            DTOExercises[3].Mode.Should().Be(ExercisesMods.FreeResult);
        }

        [Theory]
        [InlineData(ExercisesMods.Count)]
        [InlineData(ExercisesMods.WeightCount)]
        [InlineData(ExercisesMods.Timer)]
        [InlineData(ExercisesMods.FreeResult)]
        public void GetResultsExerciseFromText_WithEmptyText_ShouldThrowCreateResultExerciseException(ExercisesMods currentExerciseMode)
        {
            // Arrange
            string text = string.Empty;

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, currentExerciseMode);

            // Assert
            act.Should().Throw<CreateResultExerciseException>();
        }

        [Theory]
        [InlineData(ExercisesMods.Count)]
        [InlineData(ExercisesMods.WeightCount)]
        [InlineData(ExercisesMods.Timer)]
        public void GetResultsExerciseFromText_WithIncorrectText_ShouldThrowCreateResultExerciseException(ExercisesMods currentExerciseMode)
        {
            // Arrange
            string text = "Abc";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, currentExerciseMode);

            // Assert
            act.Should().Throw<CreateResultExerciseException>();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextWithModeCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 abc 13";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.Count);

            // Assert
            act.Should().Throw<CreateResultExerciseException>();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithTextWithModeCount_ShouldReturnExpectedResults()
        {
            // Arrange
            string text = "11 12 13";

            // Act
            List<DTOResultExercise> results = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.Count);

            // Assert
            results.Should().HaveCount(3);

            results[0].Count.Should().Be(11);
            results[0].Weight.Should().BeNull();
            results[0].DateTime.Date.Should().Be(DateTime.Now.Date);
            results[0].FreeResult.Should().BeNull();

            results[1].Count.Should().Be(12);
            results[1].Weight.Should().BeNull();
            results[1].DateTime.Date.Should().Be(DateTime.Now.Date);
            results[1].FreeResult.Should().BeNull();

            results[2].Count.Should().Be(13);
            results[2].Weight.Should().BeNull();
            results[2].DateTime.Date.Should().Be(DateTime.Now.Date);
            results[2].FreeResult.Should().BeNull();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextWithModeWeightCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 12 13";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            act.Should().Throw<CreateResultExerciseException>();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextWeightWithModeWeightCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 12;ab 14";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            act.Should().Throw<CreateResultExerciseException>();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextCountWithModeWeightCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 12;13 ab";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            act.Should().Throw<CreateResultExerciseException>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetResultsExerciseFromText_WithTextWithModeWeightCount_ShouldThrowCreateResultExerciseException(bool isNeedAdditionalSpace)
        {
            // Arrange
            string text = isNeedAdditionalSpace ? "11 12; 13 14" : "11 12;13 14";

            // Act
            List<DTOResultExercise> results = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            results.Should().HaveCount(2);

            results[0].Weight.Should().Be(11.0f);
            results[0].Count.Should().Be(12);
            results[0].DateTime.Date.Should().Be(DateTime.Now.Date);
            results[0].FreeResult.Should().BeNull();

            results[1].Weight.Should().Be(13.0f);
            results[1].Count.Should().Be(14);
            results[1].DateTime.Date.Should().Be(DateTime.Now.Date);
            results[1].FreeResult.Should().BeNull();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithTimerMode_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "12:34";

            // Act
            Action act = () => SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.Timer);

            // Assert
            act.Should().Throw<CreateResultExerciseException>();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithTextWithModeFreeResult_ShouldReturnExpectedResults()
        {
            // Arrange
            string text = "Abcd 12:34 ...";

            // Act
            List<DTOResultExercise> results = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.FreeResult);

            // Assert
            results.Should().HaveCount(1);
            results[0].Weight.Should().BeNull();
            results[0].Count.Should().BeNull();
            results[0].DateTime.Date.Should().Be(DateTime.Now.Date);
            results[0].FreeResult.Should().Be(text);
        }

        [Fact]
        public void GetInformationAboutLastExercises_WithoutResultsExercises_ShouldReturnMessageAboutEmptyResults()
        {
            // Arrange
            IQueryable<ResultExercise> results = new List<ResultExercise>().AsQueryable();

            // Act
            string resultText = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastExercises(new DateTime(2020, 6, 1), results);

            // Assert
            resultText.Should().Be("Нет информации для данного цикла");
        }

        [Fact]
        public void GetInformationAboutLastExercises_WithResultsExercises_ShouldReturnExpectedResult()
        {
            // Arrange
            Exercise exercise1 = new Exercise() { Id = 1, Name = "Exercise1", Mode = ExercisesMods.Count };
            Exercise exercise2 = new Exercise() { Id = 2, Name = "Exercise2", Mode = ExercisesMods.WeightCount };
            Exercise exercise3 = new Exercise() { Id = 3, Name = "Exercise3", Mode = ExercisesMods.FreeResult };

            IQueryable<ResultExercise> results = new List<ResultExercise>()
            {
                new ResultExercise(){ Id = 1, Count = 1, Exercise = exercise1, ExerciseId = exercise1.Id  },
                new ResultExercise(){ Id = 2, Count = 2, Exercise = exercise1, ExerciseId = exercise1.Id  },
                
                new ResultExercise(){ Id = 3, Count = 3, Weight = 4, Exercise = exercise2, ExerciseId = exercise2.Id  },
                new ResultExercise(){ Id = 4, Count = 4, Weight = 5, Exercise = exercise2, ExerciseId = exercise2.Id  },
                
                new ResultExercise(){ Id = 5, FreeResult = "Abc1", Exercise = exercise3, ExerciseId = exercise3.Id  },
                new ResultExercise(){ Id = 6, FreeResult = "Abc2", Exercise = exercise3, ExerciseId = exercise3.Id  },
            }.AsQueryable();

            string expectedResultText = @"Дата: 01.06.2020
Упражнение: ""<b>Exercise1</b>""
Повторения: (1)
Повторения: (2)
Упражнение: ""<b>Exercise2</b>""
Повторения: (3) => Вес: (4)
Повторения: (4) => Вес: (5)
Упражнение: ""<b>Exercise3</b>""
=> Abc1
=> Abc2";

            // Act
            string resultText = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastExercises(new DateTime(2020, 6, 1), results);

            // Assert
            resultText.Should().Be(expectedResultText);
        }

        [Fact]
        public void GetInformationAboutLastDay_WithoutResultsExercises_ShouldReturnMessageAboutEmptyResults()
        {
            // Arrange
            IQueryable<ResultExercise> results = new List<ResultExercise>().AsQueryable();

            // Act
            string resultText = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastDay(results);

            // Assert
            resultText.Should().Be("Нет информации для данного дня");
        }

        [Fact]
        public void GetInformationAboutLastDay_WithResultsExercisesWithDifferentDate_ShouldReturnExpectedResult()
        {
            // Arrange
            Exercise exercise1 = new Exercise() { Id = 1, Name = "Exercise1", Mode = ExercisesMods.Count };
            Exercise exercise2 = new Exercise() { Id = 2, Name = "Exercise2", Mode = ExercisesMods.WeightCount };
            Exercise exercise3 = new Exercise() { Id = 3, Name = "Exercise3", Mode = ExercisesMods.FreeResult };

            IQueryable<ResultExercise> results = new List<ResultExercise>()
            {
                new ResultExercise(){ Id = 1, Count = 1, DateTime = new DateTime(2020, 5, 1), Exercise = exercise1, ExerciseId = exercise1.Id  },
                new ResultExercise(){ Id = 2, Count = 2, DateTime = new DateTime(2020, 5, 1), Exercise = exercise1, ExerciseId = exercise1.Id  },

                new ResultExercise(){ Id = 3, Count = 3, Weight = 4, DateTime = new DateTime(2020, 6, 1), Exercise = exercise2, ExerciseId = exercise2.Id  },
                new ResultExercise(){ Id = 4, Count = 4, Weight = 5, DateTime = new DateTime(2020, 6, 1), Exercise = exercise2, ExerciseId = exercise2.Id  },

                new ResultExercise(){ Id = 5, FreeResult = "Abc1", DateTime = new DateTime(2020, 7, 1), Exercise = exercise3, ExerciseId = exercise3.Id  },
                new ResultExercise(){ Id = 6, FreeResult = "Abc2", DateTime = new DateTime(2020, 7, 1), Exercise = exercise3, ExerciseId = exercise3.Id  },
            }.AsQueryable();

            string expectedResultText = @"Дата: ""<b>01.05.2020</b>""
Упражнение: ""<b>Exercise1</b>""
Повторения: (1)
Повторения: (2)
Дата: ""<b>01.06.2020</b>""
Упражнение: ""<b>Exercise2</b>""
Повторения: (3) => Вес: (4)
Повторения: (4) => Вес: (5)
Дата: ""<b>01.07.2020</b>""
Упражнение: ""<b>Exercise3</b>""
=> Abc1
=> Abc2";

            // Act
            string resultText = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastDay(results);

            // Assert
            resultText.Should().Be(expectedResultText);
        }

        [Fact]
        public void GetInformationAboutLastDay_WithResultsExercisesWithSameDate_ShouldReturnExpectedResult()
        {
            // Arrange
            Exercise exercise1 = new Exercise() { Id = 1, Name = "Exercise1", Mode = ExercisesMods.Count };
            Exercise exercise2 = new Exercise() { Id = 2, Name = "Exercise2", Mode = ExercisesMods.WeightCount };
            Exercise exercise3 = new Exercise() { Id = 3, Name = "Exercise3", Mode = ExercisesMods.FreeResult };

            IQueryable<ResultExercise> results = new List<ResultExercise>()
            {
                new ResultExercise(){ Id = 1, Count = 1, DateTime = new DateTime(2020, 6, 1), Exercise = exercise1, ExerciseId = exercise1.Id  },
                new ResultExercise(){ Id = 2, Count = 2, DateTime = new DateTime(2020, 6, 1), Exercise = exercise1, ExerciseId = exercise1.Id  },

                new ResultExercise(){ Id = 3, Count = 3, Weight = 4, DateTime = new DateTime(2020, 6, 1), Exercise = exercise2, ExerciseId = exercise2.Id  },
                new ResultExercise(){ Id = 4, Count = 4, Weight = 5, DateTime = new DateTime(2020, 6, 1), Exercise = exercise2, ExerciseId = exercise2.Id  },

                new ResultExercise(){ Id = 5, FreeResult = "Abc1", DateTime = new DateTime(2020, 6, 1), Exercise = exercise3, ExerciseId = exercise3.Id  },
                new ResultExercise(){ Id = 6, FreeResult = "Abc2", DateTime = new DateTime(2020, 6, 1), Exercise = exercise3, ExerciseId = exercise3.Id  },
            }.AsQueryable();

            string expectedResultText = @"Дата: ""<b>01.06.2020</b>""
Упражнение: ""<b>Exercise1</b>""
Повторения: (1)
Повторения: (2)
Упражнение: ""<b>Exercise2</b>""
Повторения: (3) => Вес: (4)
Повторения: (4) => Вес: (5)
Упражнение: ""<b>Exercise3</b>""
=> Abc1
=> Abc2";

            // Act
            string resultText = SharedExercisesAndResultsLogicHelper.GetInformationAboutLastDay(results);

            // Assert
            resultText.Should().Be(expectedResultText);
        }
    }
}