using FluentAssertions;
using Microsoft.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.Export;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.UnitTests.Builders;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Export
{
    public class JsonExportHelperTests
    {
        [Fact]
        public async Task GetJSONFile_ShouldReturnCorrectJsonFileWithoutSensitiveInfo()
        {
            // Arrange 
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder();

            UserInformation testUserInformation = await GetUserInformationWithFilledCycles(entityContextBuilder);

            EntityContext entityContext = entityContextBuilder.Build();

            IQueryable<ResultExercise> results = await GetResultExercises(entityContext);

            // Act
            RecyclableMemoryStream resultStream = await JsonExportHelper.GetJSONFile(testUserInformation.ToDTOUserInformation(), results, 0, TestContext.Current.CancellationToken);

            //Assert
            JsonDocument jsonDocument = JsonDocument.Parse(resultStream);

            JsonElement rootElement = jsonDocument.RootElement;

            rootElement.TryGetProperty("Id", out JsonElement _).Should().BeTrue();
            rootElement.TryGetProperty("UserId", out JsonElement _).Should().BeFalse();
            rootElement.TryGetProperty("FirstName", out JsonElement _).Should().BeFalse();
            rootElement.TryGetProperty("Username", out JsonElement _).Should().BeFalse();
            rootElement.TryGetProperty("WhiteList", out JsonElement _).Should().BeFalse();
            rootElement.TryGetProperty("BlackList", out JsonElement _).Should().BeFalse();
            rootElement.TryGetProperty("Cycles", out JsonElement cycles).Should().BeTrue();

            cycles.ValueKind.Should().Be(JsonValueKind.Array);

            JsonElement[] cyclesArray = cycles.EnumerateArray().ToArray();
            cyclesArray.Should().HaveCount(2);

            cyclesArray[0].GetProperty("Name").GetString().Should().Be("TestCycle1");
            cyclesArray[1].GetProperty("Name").GetString().Should().Be("TestCycle2");
        }

        [Fact]
        public async Task GetJSONFile_ShouldReturnCorrectJsonLikeExample()
        {
            // Arrange
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder();

            UserInformation testUserInformation = await GetUserInformationWithFilledCycles(entityContextBuilder);

            EntityContext entityContext = entityContextBuilder.Build();

            IQueryable<ResultExercise> results = await GetResultExercises(entityContext);

            string example = GetJsonCompletedExample();

            // Act
            RecyclableMemoryStream resultStream = await JsonExportHelper.GetJSONFile(testUserInformation.ToDTOUserInformation(), results, 0, TestContext.Current.CancellationToken);

            //Assert
            JsonDocument jsonDocument = JsonDocument.Parse(resultStream);

            string json = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // игнорирования сериализации для null свойств
                ReferenceHandler = ReferenceHandler.IgnoreCycles, // игнорировать зацикленность

                WriteIndented = true, // форматированная сериализация с табуляцией
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // Чтобы кириллические (или возможные другие) символы не экранировались
            });

            json.Should().Be(example);  
        }

        private async Task<UserInformation> GetUserInformationWithFilledCycles(EntityContextBuilder entityContextBuilder)
        {
            EntityContext entityContext = entityContextBuilder.Create()
                                                              .WithUserInformation()
                                                              .WithCycles()
                                                              .Build();

            List<Cycle> testCycles = entityContextBuilder.TestCycles;

            Cycle testCycle1 = testCycles[0];

            Day day1 = new Day { Name = "TestDay1", CycleId = testCycle1.Id };
            Day day2 = new Day { Name = "TestDay2", CycleId = testCycle1.Id };

            await entityContext.Days.AddRangeAsync([day1, day2], TestContext.Current.CancellationToken);

            await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            await entityContext.Exercises.AddRangeAsync([
                    new Exercise { Name = "TestExercise1", Mode = ExercisesMods.Count, DayId = day1.Id },
                    new Exercise { Name = "TestExercise2", Mode = ExercisesMods.WeightCount, DayId = day1.Id },
                    new Exercise { Name = "TestExercise3", Mode = ExercisesMods.Timer, DayId = day2.Id },
                    new Exercise { Name = "TestExercise4", Mode = ExercisesMods.FreeResult, DayId = day2.Id }
                ], TestContext.Current.CancellationToken);

            await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            Cycle testCycle2 = testCycles[1];

            Day day3 = new Day { Name = "TestDay3", CycleId = testCycle2.Id };
            Day day4 = new Day { Name = "TestDay4", CycleId = testCycle2.Id };

            await entityContext.Days.AddRangeAsync([day3, day4], TestContext.Current.CancellationToken);

            await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            await entityContext.Exercises.AddRangeAsync([
                    new Exercise { Name = "TestExercise5", Mode = ExercisesMods.Count, DayId = day3.Id },
                    new Exercise { Name = "TestExercise6", Mode = ExercisesMods.WeightCount, DayId = day3.Id },
                    new Exercise { Name = "TestExercise7", Mode = ExercisesMods.Timer, DayId = day4.Id },
                    new Exercise { Name = "TestExercise8", Mode = ExercisesMods.Count, DayId = day4.Id }
                ], TestContext.Current.CancellationToken);

            await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            return entityContextBuilder.TestUserInformation;
        }

        private async Task<IQueryable<ResultExercise>> GetResultExercises(EntityContext entityContext)
        {
            ResultExercise resultExercise1 = new ResultExercise()
            {
                Id = 1,
                Count = 100,
                DateTime = new DateTime(2020, 7, 1),
                ExerciseId = 5
            };

            ResultExercise resultExercise2 = new ResultExercise()
            {
                Id = 2,
                Count = 200,
                DateTime = new DateTime(2020, 7, 1),
                ExerciseId = 8
            };

            await entityContext.ResultsExercises.AddRangeAsync([resultExercise1, resultExercise2], TestContext.Current.CancellationToken);

            await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            return entityContext.ResultsExercises;
        }

        private string GetJsonCompletedExample()
        {
            string example = @"{
  ""Id"": 1,
  ""Cycles"": [
    {
      ""Id"": 1,
      ""Name"": ""TestCycle1"",
      ""Days"": [
        {
          ""Id"": 1,
          ""Name"": ""TestDay1"",
          ""Exercises"": [
            {
              ""Id"": 1,
              ""Name"": ""TestExercise1"",
              ""Mode"": 0,
              ""ResultsExercise"": [],
              ""DayId"": 1,
              ""IsArchive"": false
            },
            {
              ""Id"": 2,
              ""Name"": ""TestExercise2"",
              ""Mode"": 1,
              ""ResultsExercise"": [],
              ""DayId"": 1,
              ""IsArchive"": false
            }
          ],
          ""CycleId"": 1,
          ""IsArchive"": false
        },
        {
          ""Id"": 2,
          ""Name"": ""TestDay2"",
          ""Exercises"": [
            {
              ""Id"": 3,
              ""Name"": ""TestExercise3"",
              ""Mode"": 2,
              ""ResultsExercise"": [],
              ""DayId"": 2,
              ""IsArchive"": false
            },
            {
              ""Id"": 4,
              ""Name"": ""TestExercise4"",
              ""Mode"": 3,
              ""ResultsExercise"": [],
              ""DayId"": 2,
              ""IsArchive"": false
            }
          ],
          ""CycleId"": 1,
          ""IsArchive"": false
        }
      ],
      ""UserInformationId"": 1,
      ""IsActive"": false,
      ""IsArchive"": false
    },
    {
      ""Id"": 2,
      ""Name"": ""TestCycle2"",
      ""Days"": [
        {
          ""Id"": 3,
          ""Name"": ""TestDay3"",
          ""Exercises"": [
            {
              ""Id"": 5,
              ""Name"": ""TestExercise5"",
              ""Mode"": 0,
              ""ResultsExercise"": [
                {
                  ""Id"": 1,
                  ""Count"": 100,
                  ""DateTime"": ""2020-07-01T00:00:00"",
                  ""ExerciseId"": 5
                }
              ],
              ""DayId"": 3,
              ""IsArchive"": false
            },
            {
              ""Id"": 6,
              ""Name"": ""TestExercise6"",
              ""Mode"": 1,
              ""ResultsExercise"": [],
              ""DayId"": 3,
              ""IsArchive"": false
            }
          ],
          ""CycleId"": 2,
          ""IsArchive"": false
        },
        {
          ""Id"": 4,
          ""Name"": ""TestDay4"",
          ""Exercises"": [
            {
              ""Id"": 7,
              ""Name"": ""TestExercise7"",
              ""Mode"": 2,
              ""ResultsExercise"": [],
              ""DayId"": 4,
              ""IsArchive"": false
            },
            {
              ""Id"": 8,
              ""Name"": ""TestExercise8"",
              ""Mode"": 0,
              ""ResultsExercise"": [
                {
                  ""Id"": 2,
                  ""Count"": 200,
                  ""DateTime"": ""2020-07-01T00:00:00"",
                  ""ExerciseId"": 8
                }
              ],
              ""DayId"": 4,
              ""IsArchive"": false
            }
          ],
          ""CycleId"": 2,
          ""IsArchive"": false
        }
      ],
      ""UserInformationId"": 1,
      ""IsActive"": false,
      ""IsArchive"": false
    }
  ]
}";

            return example;
        }
    }
}