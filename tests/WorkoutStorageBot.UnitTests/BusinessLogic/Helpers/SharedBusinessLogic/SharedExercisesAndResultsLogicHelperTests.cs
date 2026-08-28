using FluentAssertions;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
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
        public void GetExercisesFromText_WithEmptyText_ShouldReturnError()
        {
            // Arrange
            string text = string.Empty;

            // Act
            (List<DTOExercise> DTOExercises, string? Error) = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            DTOExercises.Should().BeEmpty();
            Error.Should().Be("Не удалось начать парсинг строки для создания упражнений, т.к. получена пустая строка");
        }

        [Fact]
        public void GetExercisesFromText_WithTextWithoutExerciseType_ShouldReturnErrorn()
        {
            // Arrange
            string text = "Exercise0-;Exercise1-1";

            // Act
            (List<DTOExercise> Exercises, string? Error) = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            Exercises.Should().BeEmpty();
            Error.Should().Be("Не удалось получить тип упражнения 'Exercise0'");
        }

        [Fact]
        public void GetExercisesFromText_WithTextWithoutExerciseName_ShouldReturnError()
        {
            // Arrange
            string text = "Exercise0-0;-1";

            // Act
            (List<DTOExercise> DTOExercises, string? Error) = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            DTOExercises.Should().BeEmpty();
            Error.Should().Be("Не удалось получить тип упражнения '1'");
        }

        [Fact]
        public void GetExercisesFromText_WithTextWithBadExerciseType_ShouldReturnError()
        {
            // Arrange
            string text = "Exercise0-10";

            // Act
            (List<DTOExercise> DTOExercises, string? Error) = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

            // Assert
            DTOExercises.Should().BeEmpty();
            Error.Should().Be("Указанный тип (10) упражнения 'Exercise0' не относится к допустимым");
        }

        [Fact]
        public void GetExercisesFromText_WithOneExerciseText_ShouldReturnOneExercise()
        {
            // Arrange
            string text = "Exercise0-0";

            // Act
            (List<DTOExercise> DTOExercises, string? Error) = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

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
            (List<DTOExercise> DTOExercises, string? Error) = SharedExercisesAndResultsLogicHelper.GetExercisesFromText(text);

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
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, currentExerciseMode);

            // Assert
            result.Error.Should().NotBeNullOrWhiteSpace();
            result.ResultsExercise.Should().BeEmpty();
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
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, currentExerciseMode);

            // Assert
            result.Error.Should().NotBeNullOrWhiteSpace();
            result.ResultsExercise.Should().BeEmpty();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextWithModeCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 abc 13";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.Count);

            // Assert
            result.Error.Should().NotBeNullOrWhiteSpace();
            result.ResultsExercise.Should().BeEmpty();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithTextWithModeCount_ShouldReturnExpectedResults()
        {
            // Arrange
            string text = "11 12 13";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.Count);

            // Assert
            result.ResultsExercise.Should().HaveCount(3);

            result.ResultsExercise[0].Count.Should().Be(11);
            result.ResultsExercise[0].Weight.Should().BeNull();
            result.ResultsExercise[0].DateTime.Date.Should().Be(DateTime.Now.Date);
            result.ResultsExercise[0].FreeResult.Should().BeNull();

            result.ResultsExercise[1].Count.Should().Be(12);
            result.ResultsExercise[1].Weight.Should().BeNull();
            result.ResultsExercise[1].DateTime.Date.Should().Be(DateTime.Now.Date);
            result.ResultsExercise[1].FreeResult.Should().BeNull();

            result.ResultsExercise[2].Count.Should().Be(13);
            result.ResultsExercise[2].Weight.Should().BeNull();
            result.ResultsExercise[2].DateTime.Date.Should().Be(DateTime.Now.Date);
            result.ResultsExercise[2].FreeResult.Should().BeNull();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextWithModeWeightCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 12 13";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            result.Error.Should().NotBeNullOrWhiteSpace();
            result.ResultsExercise.Should().BeEmpty();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextWeightWithModeWeightCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 12;ab 14";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            result.Error.Should().NotBeNullOrWhiteSpace();
            result.ResultsExercise.Should().BeEmpty();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithIncorrectTextCountWithModeWeightCount_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "11 12;13 ab";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            result.Error.Should().NotBeNullOrWhiteSpace();
            result.ResultsExercise.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetResultsExerciseFromText_WithTextWithModeWeightCount_ShouldThrowCreateResultExerciseException(bool isNeedAdditionalSpace)
        {
            // Arrange
            string text = isNeedAdditionalSpace ? "11 12; 13 14" : "11 12;13 14";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.WeightCount);

            // Assert
            result.ResultsExercise.Should().HaveCount(2);

            result.ResultsExercise[0].Weight.Should().Be(11.0f);
            result.ResultsExercise[0].Count.Should().Be(12);
            result.ResultsExercise[0].DateTime.Date.Should().Be(DateTime.Now.Date);
            result.ResultsExercise[0].FreeResult.Should().BeNull();

            result.ResultsExercise[1].Weight.Should().Be(13.0f);
            result.ResultsExercise[1].Count.Should().Be(14);
            result.ResultsExercise[1].DateTime.Date.Should().Be(DateTime.Now.Date);
            result.ResultsExercise[1].FreeResult.Should().BeNull();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithTimerMode_ShouldThrowCreateResultExerciseException()
        {
            // Arrange
            string text = "12:34";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.Timer);

            // Assert
            result.Error.Should().NotBeNullOrWhiteSpace();
            result.ResultsExercise.Should().BeEmpty();
        }

        [Fact]
        public void GetResultsExerciseFromText_WithTextWithModeFreeResult_ShouldReturnExpectedResults()
        {
            // Arrange
            string text = "Abcd 12:34 ...";

            // Act
            (List<DTOResultExercise> ResultsExercise, string? Error) result = SharedExercisesAndResultsLogicHelper.GetResultsExerciseFromText(text, ExercisesMods.FreeResult);

            // Assert
            result.ResultsExercise.Should().HaveCount(1);
            result.ResultsExercise[0].Weight.Should().BeNull();
            result.ResultsExercise[0].Count.Should().BeNull();
            result.ResultsExercise[0].DateTime.Date.Should().Be(DateTime.Now.Date);
            result.ResultsExercise[0].FreeResult.Should().Be(text);
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