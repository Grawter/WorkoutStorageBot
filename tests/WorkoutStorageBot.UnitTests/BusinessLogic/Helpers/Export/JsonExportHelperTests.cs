using FluentAssertions;
using Microsoft.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WorkoutStorageBot.BusinessLogic.Helpers.Export;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.UnitTests.BusinessLogic.Helpers.Export
{
    public class JsonExportHelperTests
    {
        [Fact]
        public async Task GetJSONFile_ShouldReturnCorrectJsonFileWithoutSensitiveInfo()
        {
            // Arrange 
            DTOUserInformation DTOTestUserInformation = GetDTOUserInformationWithCycles();

            IQueryable<ResultExercise> results = GetResultExercises();

            // Act
            RecyclableMemoryStream resultStream = await JsonExportHelper.GetJSONFile(DTOTestUserInformation, results, 0);

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

            cyclesArray[0].GetProperty("Name").GetString().Should().Be("DTOCycle1");
            cyclesArray[1].GetProperty("Name").GetString().Should().Be("DTOCycle2");
        }

        [Fact]
        public async Task GetJSONFile_ShouldReturnCorrectJsonLikeExample()
        {
            // Arrange
            string example = GetJsonCompletedExample(); 

            DTOUserInformation DTOTestUserInformation = GetDTOUserInformationWithCycles();

            IQueryable<ResultExercise> results = GetResultExercises();

            // Act
            RecyclableMemoryStream resultStream = await JsonExportHelper.GetJSONFile(DTOTestUserInformation, results, 0);

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

        private DTOUserInformation GetDTOUserInformationWithCycles()
        {
            DTOModelBuilder DTOModelBuilder = new DTOModelBuilder().WithDTOUserInformation()
                                                                   .WithDTOCyclesByDTOUserInformation();

            List<DTOCycle> DTOTestCycles = DTOModelBuilder.DTOTestCycles;

            DTOCycle DTOCycle1 = DTOTestCycles[0];

            DTOCycle1.Days =
                [
                    new DTODay()
                    {
                        Id = 1,
                        Name = "DTODay1",
                        CycleId = DTOCycle1.Id,
                        Exercises =
                        [
                            new DTOExercise() { Id = 1, Name = "DTOExercise1" , Mode = ExercisesMods.Count },
                            new DTOExercise() { Id = 2, Name = "DTOExercise2" , Mode = ExercisesMods.WeightCount },
                        ]
                    },
                    new DTODay()
                    {
                        Id = 2,
                        Name = "DTODay2",
                        CycleId = DTOCycle1.Id,
                        Exercises =
                        [
                            new DTOExercise() { Id = 3, Name = "DTOExercise3" , Mode = ExercisesMods.Timer },
                            new DTOExercise() { Id = 4, Name = "DTOExercise4" , Mode = ExercisesMods.FreeResult },
                        ]
                    },
                ];

            DTOCycle DTOCycle2 = DTOTestCycles[1];

            DTOCycle2.Days =
                [
                    new DTODay()
                    {
                        Id = 3,
                        Name = "DTODay3",
                        CycleId = DTOCycle1.Id,
                        Exercises =
                        [
                            new DTOExercise() { Id = 5, Name = "DTOExercise5" , Mode = ExercisesMods.Count },
                            new DTOExercise() { Id = 6, Name = "DTOExercise6" , Mode = ExercisesMods.Count },
                        ]
                    },
                    new DTODay()
                    {
                        Id = 4,
                        Name = "DTODay4",
                        CycleId = DTOCycle1.Id,
                        Exercises =
                        [
                            new DTOExercise() { Id = 7, Name = "DTOExercise7" , Mode = ExercisesMods.Count },
                            new DTOExercise() { Id = 8, Name = "DTOExercise8" , Mode = ExercisesMods.Count },
                        ]
                    },
                ];

            return DTOModelBuilder.DTOTestUserInformation;
        }

        private IQueryable<ResultExercise> GetResultExercises()
        {
            IQueryable<ResultExercise> results = new List<ResultExercise>()
            {
                new ResultExercise()
                {
                    Id = 1,
                    Count = 100,
                    DateTime = new DateTime(2020, 7, 1),
                    ExerciseId = 5
                },
                new ResultExercise()
                {
                    Id = 2,
                    Count = 200,
                    DateTime = new DateTime(2020, 7, 1),
                    ExerciseId = 8
                }
            }.AsQueryable();

            return results;
        }

        private string GetJsonCompletedExample()
        {
            string example = @"{
  ""Id"": 1,
  ""Cycles"": [
    {
      ""Id"": 1,
      ""Name"": ""DTOCycle1"",
      ""Days"": [
        {
          ""Id"": 1,
          ""Name"": ""DTODay1"",
          ""Exercises"": [
            {
              ""Id"": 1,
              ""Name"": ""DTOExercise1"",
              ""Mode"": 0,
              ""ResultsExercise"": [],
              ""DayId"": 0,
              ""IsArchive"": false
            },
            {
              ""Id"": 2,
              ""Name"": ""DTOExercise2"",
              ""Mode"": 1,
              ""ResultsExercise"": [],
              ""DayId"": 0,
              ""IsArchive"": false
            }
          ],
          ""CycleId"": 1,
          ""IsArchive"": false
        },
        {
          ""Id"": 2,
          ""Name"": ""DTODay2"",
          ""Exercises"": [
            {
              ""Id"": 3,
              ""Name"": ""DTOExercise3"",
              ""Mode"": 2,
              ""ResultsExercise"": [],
              ""DayId"": 0,
              ""IsArchive"": false
            },
            {
              ""Id"": 4,
              ""Name"": ""DTOExercise4"",
              ""Mode"": 3,
              ""ResultsExercise"": [],
              ""DayId"": 0,
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
      ""Name"": ""DTOCycle2"",
      ""Days"": [
        {
          ""Id"": 3,
          ""Name"": ""DTODay3"",
          ""Exercises"": [
            {
              ""Id"": 5,
              ""Name"": ""DTOExercise5"",
              ""Mode"": 0,
              ""ResultsExercise"": [
                {
                  ""Id"": 1,
                  ""Count"": 100,
                  ""DateTime"": ""2020-07-01T00:00:00"",
                  ""ExerciseId"": 5
                }
              ],
              ""DayId"": 0,
              ""IsArchive"": false
            },
            {
              ""Id"": 6,
              ""Name"": ""DTOExercise6"",
              ""Mode"": 0,
              ""ResultsExercise"": [],
              ""DayId"": 0,
              ""IsArchive"": false
            }
          ],
          ""CycleId"": 1,
          ""IsArchive"": false
        },
        {
          ""Id"": 4,
          ""Name"": ""DTODay4"",
          ""Exercises"": [
            {
              ""Id"": 7,
              ""Name"": ""DTOExercise7"",
              ""Mode"": 0,
              ""ResultsExercise"": [],
              ""DayId"": 0,
              ""IsArchive"": false
            },
            {
              ""Id"": 8,
              ""Name"": ""DTOExercise8"",
              ""Mode"": 0,
              ""ResultsExercise"": [
                {
                  ""Id"": 2,
                  ""Count"": 200,
                  ""DateTime"": ""2020-07-01T00:00:00"",
                  ""ExerciseId"": 8
                }
              ],
              ""DayId"": 0,
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
    }
  ]
}";

            return example;
        }
    }
}