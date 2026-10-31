using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using OfficeOpenXml;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Helpers.Export;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.UnitTests.Helpers;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.BusinessLogic.Helpers.Export
{
    public class ExcelExportHelperTests
    {
        [Fact]
        public async Task GetExcelFile_WithCyclesAndResultExercises_ShouldReturnExpectedExcelFile()
        {
            // Arrange 
            using EntityContextBuilder entityContextBuilder = new EntityContextBuilder();
            EntityContext entityContext = entityContextBuilder.Create()
                                                              .Build();

            // Arrange
            UserInformation testUserInformation = await GetUserInformationWithCycles(entityContext);

            IQueryable<ResultExercise> results = await GetResultExercises(entityContext);

            // Act
            RecyclableMemoryStream excelFileStream  = await ExcelExportHelper.GetExcelFile(testUserInformation.ToDTOUserInformation().Cycles, results, 0, TestContext.Current.CancellationToken);

            using ExcelPackage excelPackage = new ExcelPackage(excelFileStream);

            // Assert
            ExcelRange cells = excelPackage.Workbook.Worksheets[0].Cells;

            #region cycle1 одинаковая дата у упражнений

            Cycle cycle1 = testUserInformation.Cycles[0];
            Day day11 = cycle1.Days[0];
            Exercise exercise111 = day11.Exercises[0];
            Exercise exercise112 = day11.Exercises[1];
            Exercise exercise113 = day11.Exercises[2];

            Day day12 = cycle1.Days[1];
            Exercise exercise121 = day12.Exercises[0];
            Exercise exercise122 = day12.Exercises[1];
            Exercise exercise123 = day12.Exercises[2];

            Day day13 = cycle1.Days[2];
            Exercise exercise131 = day13.Exercises[0];
            Exercise exercise132 = day13.Exercises[1];
            Exercise exercise133 = day13.Exercises[2];

            cells.GetCellValue<string>(3, 0).Should().Be(cycle1.Name);

            cells.GetCellValue<string>(4, 0).Should().Be(day11.Name);
            cells.GetCellValue<string>(4, 12).Should().Be(day12.Name);
            cells.GetCellValue<string>(4, 24).Should().Be(day13.Name);

            cells.GetCellValue<string>(5, 0).Should().Be(exercise111.Name);
            cells.GetCellValue<string>(5, 4).Should().Be(exercise112.Name);
            cells.GetCellValue<string>(5, 8).Should().Be(exercise113.Name);

            cells.GetCellValue<string>(5, 12).Should().Be(exercise121.Name);
            cells.GetCellValue<string>(5, 16).Should().Be(exercise122.Name);
            cells.GetCellValue<string>(5, 20).Should().Be(exercise123.Name);

            cells.GetCellValue<string>(5, 24).Should().Be(exercise131.Name);
            cells.GetCellValue<string>(5, 28).Should().Be(exercise132.Name);
            cells.GetCellValue<string>(5, 32).Should().Be(exercise133.Name);

            cells.GetCellValue<DateTime>(7, 0).Should().Be(exercise111.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 1).Should().Be(exercise111.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 1).Should().Be(exercise111.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 1).Should().Be(exercise111.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 4).Should().Be(exercise112.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 5).Should().Be(exercise112.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 5).Should().Be(exercise112.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 5).Should().Be(exercise112.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 8).Should().Be(exercise113.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 9).Should().Be(exercise113.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 9).Should().Be(exercise113.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 9).Should().Be(exercise113.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 12).Should().Be(exercise121.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 13).Should().Be(exercise121.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 13).Should().Be(exercise121.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 13).Should().Be(exercise121.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 16).Should().Be(exercise122.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 17).Should().Be(exercise122.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 17).Should().Be(exercise122.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 17).Should().Be(exercise122.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 20).Should().Be(exercise123.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 21).Should().Be(exercise123.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 21).Should().Be(exercise123.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 21).Should().Be(exercise123.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 24).Should().Be(exercise131.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 25).Should().Be(exercise131.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 25).Should().Be(exercise131.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 25).Should().Be(exercise131.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 28).Should().Be(exercise132.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 29).Should().Be(exercise132.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 29).Should().Be(exercise132.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 29).Should().Be(exercise132.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(7, 32).Should().Be(exercise133.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(7, 33).Should().Be(exercise133.ResultsExercise[0].Count);
            cells.GetCellValue<int>(8, 33).Should().Be(exercise133.ResultsExercise[1].Count);
            cells.GetCellValue<int>(9, 33).Should().Be(exercise133.ResultsExercise[2].Count);

            #endregion

            #region cycle2 одинаковая дата у упражнений

            Cycle cycle2 = testUserInformation.Cycles[1];
            Day day21 = cycle2.Days[0];
            Exercise exercise211 = day21.Exercises[0];
            Exercise exercise212 = day21.Exercises[1];
            Exercise exercise213 = day21.Exercises[2];

            Day day22 = cycle2.Days[1];
            Exercise exercise221 = day22.Exercises[0];
            Exercise exercise222 = day22.Exercises[1];
            Exercise exercise223 = day22.Exercises[2];

            Day day23 = cycle2.Days[2];
            Exercise exercise231 = day23.Exercises[0];
            Exercise exercise232 = day23.Exercises[1];
            Exercise exercise233 = day23.Exercises[2];

            cells.GetCellValue<string>(13, 0).Should().Be(cycle2.Name);

            cells.GetCellValue<string>(14, 0).Should().Be(day21.Name);
            cells.GetCellValue<string>(14, 12).Should().Be(day22.Name);
            cells.GetCellValue<string>(14, 24).Should().Be(day23.Name);

            cells.GetCellValue<string>(15, 0).Should().Be(exercise211.Name);
            cells.GetCellValue<string>(15, 4).Should().Be(exercise212.Name);
            cells.GetCellValue<string>(15, 8).Should().Be(exercise213.Name);

            cells.GetCellValue<string>(15, 12).Should().Be(exercise221.Name);
            cells.GetCellValue<string>(15, 16).Should().Be(exercise222.Name);
            cells.GetCellValue<string>(15, 20).Should().Be(exercise223.Name);

            cells.GetCellValue<string>(15, 24).Should().Be(exercise231.Name);
            cells.GetCellValue<string>(15, 28).Should().Be(exercise232.Name);
            cells.GetCellValue<string>(15, 32).Should().Be(exercise233.Name);

            cells.GetCellValue<DateTime>(17, 0).Should().Be(exercise211.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 1).Should().Be(exercise211.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 1).Should().Be(exercise211.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 1).Should().Be(exercise211.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 4).Should().Be(exercise212.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 5).Should().Be(exercise212.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 5).Should().Be(exercise212.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 5).Should().Be(exercise212.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 8).Should().Be(exercise213.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 9).Should().Be(exercise213.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 9).Should().Be(exercise213.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 9).Should().Be(exercise213.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 12).Should().Be(exercise221.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 13).Should().Be(exercise221.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 13).Should().Be(exercise221.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 13).Should().Be(exercise221.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 16).Should().Be(exercise222.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 17).Should().Be(exercise222.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 17).Should().Be(exercise222.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 17).Should().Be(exercise222.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 20).Should().Be(exercise223.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 21).Should().Be(exercise223.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 21).Should().Be(exercise223.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 21).Should().Be(exercise223.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 24).Should().Be(exercise231.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 25).Should().Be(exercise231.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 25).Should().Be(exercise231.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 25).Should().Be(exercise231.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 28).Should().Be(exercise232.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 29).Should().Be(exercise232.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 29).Should().Be(exercise232.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 29).Should().Be(exercise232.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(17, 32).Should().Be(exercise233.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(17, 33).Should().Be(exercise233.ResultsExercise[0].Count);
            cells.GetCellValue<int>(18, 33).Should().Be(exercise233.ResultsExercise[1].Count);
            cells.GetCellValue<int>(19, 33).Should().Be(exercise233.ResultsExercise[2].Count);

            #endregion

            #region cycle3 разная дата у упражнений

            Cycle cycle3 = testUserInformation.Cycles[2];
            Day day31 = cycle3.Days[0];
            Exercise exercise311 = day31.Exercises[0];
            Exercise exercise312 = day31.Exercises[1];
            Exercise exercise313 = day31.Exercises[2];

            Day day32 = cycle3.Days[1];
            Exercise exercise321 = day32.Exercises[0];
            Exercise exercise322 = day32.Exercises[1];
            Exercise exercise323 = day32.Exercises[2];

            Day day33 = cycle3.Days[2];
            Exercise exercise331 = day33.Exercises[0];
            Exercise exercise332 = day33.Exercises[1];
            Exercise exercise333 = day33.Exercises[2];

            cells.GetCellValue<string>(23, 0).Should().Be(cycle3.Name);

            cells.GetCellValue<string>(24, 0).Should().Be(day31.Name);
            cells.GetCellValue<string>(24, 12).Should().Be(day32.Name);
            cells.GetCellValue<string>(24, 24).Should().Be(day33.Name);

            cells.GetCellValue<string>(25, 0).Should().Be(exercise311.Name);
            cells.GetCellValue<string>(25, 4).Should().Be(exercise312.Name);
            cells.GetCellValue<string>(25, 8).Should().Be(exercise313.Name);

            cells.GetCellValue<string>(25, 12).Should().Be(exercise321.Name);
            cells.GetCellValue<string>(25, 16).Should().Be(exercise322.Name);
            cells.GetCellValue<string>(25, 20).Should().Be(exercise323.Name);

            cells.GetCellValue<string>(25, 24).Should().Be(exercise331.Name);
            cells.GetCellValue<string>(25, 28).Should().Be(exercise332.Name);
            cells.GetCellValue<string>(25, 32).Should().Be(exercise333.Name);

            cells.GetCellValue<DateTime>(27, 0).Should().Be(exercise311.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 1).Should().Be(exercise311.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 0).Should().Be(exercise311.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 1).Should().Be(exercise311.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 0).Should().Be(exercise311.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 1).Should().Be(exercise311.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 4).Should().Be(exercise312.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 5).Should().Be(exercise312.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 4).Should().Be(exercise312.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 5).Should().Be(exercise312.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 4).Should().Be(exercise312.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 5).Should().Be(exercise312.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 8).Should().Be(exercise313.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 9).Should().Be(exercise313.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 8).Should().Be(exercise313.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 9).Should().Be(exercise313.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 8).Should().Be(exercise313.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 9).Should().Be(exercise313.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 12).Should().Be(exercise321.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 13).Should().Be(exercise321.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 12).Should().Be(exercise321.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 13).Should().Be(exercise321.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 12).Should().Be(exercise321.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 13).Should().Be(exercise321.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 16).Should().Be(exercise322.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 17).Should().Be(exercise322.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 16).Should().Be(exercise322.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 17).Should().Be(exercise322.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 16).Should().Be(exercise322.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 17).Should().Be(exercise322.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 20).Should().Be(exercise323.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 21).Should().Be(exercise323.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 20).Should().Be(exercise323.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 21).Should().Be(exercise323.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 20).Should().Be(exercise323.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 21).Should().Be(exercise323.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 24).Should().Be(exercise331.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 25).Should().Be(exercise331.ResultsExercise[0].Count);
            cells.GetCellValue<int>(28, 25).Should().Be(exercise331.ResultsExercise[1].Count);
            cells.GetCellValue<int>(29, 25).Should().Be(exercise331.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 28).Should().Be(exercise332.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 29).Should().Be(exercise332.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 28).Should().Be(exercise332.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 29).Should().Be(exercise332.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 28).Should().Be(exercise332.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 29).Should().Be(exercise332.ResultsExercise[2].Count);

            cells.GetCellValue<DateTime>(27, 32).Should().Be(exercise333.ResultsExercise[0].DateTime.Date);
            cells.GetCellValue<int>(27, 33).Should().Be(exercise333.ResultsExercise[0].Count);
            cells.GetCellValue<DateTime>(28, 32).Should().Be(exercise333.ResultsExercise[1].DateTime.Date);
            cells.GetCellValue<int>(28, 33).Should().Be(exercise333.ResultsExercise[1].Count);
            cells.GetCellValue<DateTime>(29, 32).Should().Be(exercise333.ResultsExercise[2].DateTime.Date);
            cells.GetCellValue<int>(29, 33).Should().Be(exercise333.ResultsExercise[2].Count);

            #endregion
        }

        private async Task<UserInformation> GetUserInformationWithCycles(EntityContext entityContext)
        {
            UserInformation userInformation = new UserInformation
            {
                FirstName = "FirstName",
                Username = "Username",
                Cycles = new List<Cycle>
                {
                    new Cycle
                    {
                        Name = "Cycle1",
                        Days = new List<Day>
                        {
                            new Day
                            {
                                Name = "Day11",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise111" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise112" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise113" },
                                }
                            },
                            new Day
                            {
                                Name = "Day12",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise121" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise122" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise123" },
                                }
                            },
                            new Day
                            {
                                Name = "Day13",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise131" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise132" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise133" },
                                }
                            }
                        }
                    },
                    new Cycle
                    {
                        Name = "Cycle2",
                        Days = new List<Day>
                        {
                            new Day
                            {
                                Name = "Day21",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise211" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise212" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise213" },
                                }
                            },
                            new Day
                            {
                                Name = "Day22",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise221" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise222" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise223" },
                                }
                            },
                            new Day
                            {
                                Name = "Day23",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise231" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise232" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise233" },
                                }
                            }
                        }
                    },
                    new Cycle
                    {
                        Name = "Cycle3",
                        Days = new List<Day>
                        {
                            new Day
                            {
                                Name = "Day31",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise311" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise312" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise313" },
                                }
                            },
                            new Day
                            {
                                Name = "Day32",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise321" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise322" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise323" },
                                }
                            },
                            new Day
                            {
                                Name = "Day33",
                                Exercises = new List<Exercise>
                                {
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise331" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise332" },
                                    new Exercise { Mode = ExercisesMods.Count, Name = "Exercise333" },
                                }
                            }
                        }
                    }
                }
            };

            await entityContext.UsersInformation.AddAsync(userInformation, TestContext.Current.CancellationToken);

            // Сохраняем один раз — EF вставит граф в правильном порядке
            // и проставит сгенерированные ключи (Exercise.Id, Day.Id, Cycle.Id, UserInformation.Id).
            await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            return userInformation;
        }

        private async Task<IQueryable<ResultExercise>> GetResultExercises(EntityContext entityContext)
        {
            IList<int> exerciseIds = await entityContext.Exercises
                .OrderBy(e => e.Id)
                .Select(e => e.Id)
                .Take(27)
                .ToListAsync(TestContext.Current.CancellationToken);

            if (exerciseIds.Count < 27)
            {
                throw new InvalidOperationException(
                    "Ожидалось 27 упражнений в БД. Сначала вызовите GetDTOUserInformationWithCyclesAsync.");
            }

            var results = new List<ResultExercise>
            {
                new ResultExercise { ExerciseId = exerciseIds[0],  DateTime = DateTime.Now, Count = 10 },
                new ResultExercise { ExerciseId = exerciseIds[0],  DateTime = DateTime.Now, Count = 20 },
                new ResultExercise { ExerciseId = exerciseIds[0],  DateTime = DateTime.Now, Count = 30 },
                new ResultExercise { ExerciseId = exerciseIds[1],  DateTime = DateTime.Now, Count = 40 },
                new ResultExercise { ExerciseId = exerciseIds[1],  DateTime = DateTime.Now, Count = 50 },
                new ResultExercise { ExerciseId = exerciseIds[1],  DateTime = DateTime.Now, Count = 60 },
                new ResultExercise { ExerciseId = exerciseIds[2],  DateTime = DateTime.Now, Count = 70 },
                new ResultExercise { ExerciseId = exerciseIds[2],  DateTime = DateTime.Now, Count = 80 },
                new ResultExercise { ExerciseId = exerciseIds[2],  DateTime = DateTime.Now, Count = 90 },
                new ResultExercise { ExerciseId = exerciseIds[3],  DateTime = DateTime.Now, Count = 100 },
                new ResultExercise { ExerciseId = exerciseIds[3],  DateTime = DateTime.Now, Count = 110 },
                new ResultExercise { ExerciseId = exerciseIds[3],  DateTime = DateTime.Now, Count = 120 },
                new ResultExercise { ExerciseId = exerciseIds[4],  DateTime = DateTime.Now, Count = 130 },
                new ResultExercise { ExerciseId = exerciseIds[4],  DateTime = DateTime.Now, Count = 140 },
                new ResultExercise { ExerciseId = exerciseIds[4],  DateTime = DateTime.Now, Count = 150 },
                new ResultExercise { ExerciseId = exerciseIds[5],  DateTime = DateTime.Now, Count = 160 },
                new ResultExercise { ExerciseId = exerciseIds[5],  DateTime = DateTime.Now, Count = 170 },
                new ResultExercise { ExerciseId = exerciseIds[5],  DateTime = DateTime.Now, Count = 180 },
                new ResultExercise { ExerciseId = exerciseIds[6],  DateTime = DateTime.Now, Count = 190 },
                new ResultExercise { ExerciseId = exerciseIds[6],  DateTime = DateTime.Now, Count = 200 },
                new ResultExercise { ExerciseId = exerciseIds[6],  DateTime = DateTime.Now, Count = 210 },
                new ResultExercise { ExerciseId = exerciseIds[7],  DateTime = DateTime.Now, Count = 220 },
                new ResultExercise { ExerciseId = exerciseIds[7],  DateTime = DateTime.Now, Count = 230 },
                new ResultExercise { ExerciseId = exerciseIds[7],  DateTime = DateTime.Now, Count = 240 },
                new ResultExercise { ExerciseId = exerciseIds[8],  DateTime = DateTime.Now, Count = 250 },
                new ResultExercise { ExerciseId = exerciseIds[8],  DateTime = DateTime.Now, Count = 260 },
                new ResultExercise { ExerciseId = exerciseIds[8],  DateTime = DateTime.Now, Count = 270 },
                new ResultExercise { ExerciseId = exerciseIds[9],  DateTime = DateTime.Now, Count = 280 },
                new ResultExercise { ExerciseId = exerciseIds[9],  DateTime = DateTime.Now, Count = 290 },
                new ResultExercise { ExerciseId = exerciseIds[9],  DateTime = DateTime.Now, Count = 300 },
                new ResultExercise { ExerciseId = exerciseIds[10], DateTime = DateTime.Now, Count = 310 },
                new ResultExercise { ExerciseId = exerciseIds[10], DateTime = DateTime.Now, Count = 320 },
                new ResultExercise { ExerciseId = exerciseIds[10], DateTime = DateTime.Now, Count = 330 },
                new ResultExercise { ExerciseId = exerciseIds[11], DateTime = DateTime.Now, Count = 340 },
                new ResultExercise { ExerciseId = exerciseIds[11], DateTime = DateTime.Now, Count = 350 },
                new ResultExercise { ExerciseId = exerciseIds[11], DateTime = DateTime.Now, Count = 360 },
                new ResultExercise { ExerciseId = exerciseIds[12], DateTime = DateTime.Now, Count = 370 },
                new ResultExercise { ExerciseId = exerciseIds[12], DateTime = DateTime.Now, Count = 380 },
                new ResultExercise { ExerciseId = exerciseIds[12], DateTime = DateTime.Now, Count = 390 },
                new ResultExercise { ExerciseId = exerciseIds[13], DateTime = DateTime.Now, Count = 400 },
                new ResultExercise { ExerciseId = exerciseIds[13], DateTime = DateTime.Now, Count = 410 },
                new ResultExercise { ExerciseId = exerciseIds[13], DateTime = DateTime.Now, Count = 420 },
                new ResultExercise { ExerciseId = exerciseIds[14], DateTime = DateTime.Now, Count = 430 },
                new ResultExercise { ExerciseId = exerciseIds[14], DateTime = DateTime.Now, Count = 440 },
                new ResultExercise { ExerciseId = exerciseIds[14], DateTime = DateTime.Now, Count = 450 },
                new ResultExercise { ExerciseId = exerciseIds[15], DateTime = DateTime.Now, Count = 460 },
                new ResultExercise { ExerciseId = exerciseIds[15], DateTime = DateTime.Now, Count = 470 },
                new ResultExercise { ExerciseId = exerciseIds[15], DateTime = DateTime.Now, Count = 480 },
                new ResultExercise { ExerciseId = exerciseIds[16], DateTime = DateTime.Now, Count = 490 },
                new ResultExercise { ExerciseId = exerciseIds[16], DateTime = DateTime.Now, Count = 500 },
                new ResultExercise { ExerciseId = exerciseIds[16], DateTime = DateTime.Now, Count = 510 },
                new ResultExercise { ExerciseId = exerciseIds[17], DateTime = DateTime.Now, Count = 520 },
                new ResultExercise { ExerciseId = exerciseIds[17], DateTime = DateTime.Now, Count = 530 },
                new ResultExercise { ExerciseId = exerciseIds[17], DateTime = DateTime.Now, Count = 540 },
                new ResultExercise { ExerciseId = exerciseIds[18], DateTime = new DateTime(2020, 4, 1),  Count = 550 },
                new ResultExercise { ExerciseId = exerciseIds[18], DateTime = new DateTime(2020, 4, 2),  Count = 560 },
                new ResultExercise { ExerciseId = exerciseIds[18], DateTime = new DateTime(2020, 4, 3),  Count = 570 },
                new ResultExercise { ExerciseId = exerciseIds[19], DateTime = new DateTime(2020, 5, 1),  Count = 580 },
                new ResultExercise { ExerciseId = exerciseIds[19], DateTime = new DateTime(2020, 5, 2),  Count = 590 },
                new ResultExercise { ExerciseId = exerciseIds[19], DateTime = new DateTime(2020, 5, 3),  Count = 600 },
                new ResultExercise { ExerciseId = exerciseIds[20], DateTime = new DateTime(2020, 6, 1),  Count = 610 },
                new ResultExercise { ExerciseId = exerciseIds[20], DateTime = new DateTime(2020, 6, 2),  Count = 620 },
                new ResultExercise { ExerciseId = exerciseIds[20], DateTime = new DateTime(2020, 6, 3),  Count = 630 },
                new ResultExercise { ExerciseId = exerciseIds[21], DateTime = new DateTime(2020, 7, 1),  Count = 640 },
                new ResultExercise { ExerciseId = exerciseIds[21], DateTime = new DateTime(2020, 7, 2),  Count = 650 },
                new ResultExercise { ExerciseId = exerciseIds[21], DateTime = new DateTime(2020, 7, 3),  Count = 660 },
                new ResultExercise { ExerciseId = exerciseIds[22], DateTime = new DateTime(2020, 8, 1),  Count = 670 },
                new ResultExercise { ExerciseId = exerciseIds[22], DateTime = new DateTime(2020, 8, 2),  Count = 680 },
                new ResultExercise { ExerciseId = exerciseIds[22], DateTime = new DateTime(2020, 8, 3),  Count = 690 },
                new ResultExercise { ExerciseId = exerciseIds[23], DateTime = new DateTime(2020, 9, 1),  Count = 700 },
                new ResultExercise { ExerciseId = exerciseIds[23], DateTime = new DateTime(2020, 9, 2),  Count = 710 },
                new ResultExercise { ExerciseId = exerciseIds[23], DateTime = new DateTime(2020, 9, 3),  Count = 720 },
                new ResultExercise { ExerciseId = exerciseIds[24], DateTime = new DateTime(2020, 10, 1), Count = 730 },
                new ResultExercise { ExerciseId = exerciseIds[24], DateTime = new DateTime(2020, 10, 2), Count = 740 },
                new ResultExercise { ExerciseId = exerciseIds[24], DateTime = new DateTime(2020, 10, 3), Count = 750 },
                new ResultExercise { ExerciseId = exerciseIds[25], DateTime = new DateTime(2020, 11, 1), Count = 760 },
                new ResultExercise { ExerciseId = exerciseIds[25], DateTime = new DateTime(2020, 11, 2), Count = 770 },
                new ResultExercise { ExerciseId = exerciseIds[25], DateTime = new DateTime(2020, 11, 3), Count = 780 },
                new ResultExercise { ExerciseId = exerciseIds[26], DateTime = new DateTime(2020, 12, 1), Count = 790 },
                new ResultExercise { ExerciseId = exerciseIds[26], DateTime = new DateTime(2020, 12, 2), Count = 800 },
                new ResultExercise { ExerciseId = exerciseIds[26], DateTime = new DateTime(2020, 12, 3), Count = 810 },
            };

            await entityContext.ResultsExercises.AddRangeAsync(results, TestContext.Current.CancellationToken);

            await entityContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            return entityContext.ResultsExercises;
        }
    }
}