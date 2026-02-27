using FluentAssertions;
using Microsoft.IO;
using OfficeOpenXml;
using WorkoutStorageBot.BusinessLogic.Helpers.Export;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.IntegrationTests.BusinessLogic.Helpers.Export
{
    public class ExcelExportHelperTests
    {
        [Fact]
        public async Task GetExcelFile_WithCyclesAndResultExercises_ShouldReturnExpectedExcelFile()
        {
            // Arrange
            DTOUserInformation DTOTestUserInformation = GetDTOUserInformationWithCycles();

            IQueryable<ResultExercise> results = GetResultExercises();

            // Act
            RecyclableMemoryStream excelFileStream  = await ExcelExportHelper.GetExcelFile(DTOTestUserInformation.Cycles, results, 0);

            using ExcelPackage excelPackage = new ExcelPackage(excelFileStream);

            // Assert
            ExcelRange cells = excelPackage.Workbook.Worksheets[0].Cells;

            #region cycle1 одинаковая дата у упражнений

            DTOCycle cycle1 = DTOTestUserInformation.Cycles[0];
            DTODay day11 = cycle1.Days[0];
            DTOExercise exercise111 = day11.Exercises[0];
            DTOExercise exercise112 = day11.Exercises[1];
            DTOExercise exercise113 = day11.Exercises[2];

            DTODay day12 = cycle1.Days[1];
            DTOExercise exercise121 = day12.Exercises[0];
            DTOExercise exercise122 = day12.Exercises[1];
            DTOExercise exercise123 = day12.Exercises[2];

            DTODay day13 = cycle1.Days[2];
            DTOExercise exercise131 = day13.Exercises[0];
            DTOExercise exercise132 = day13.Exercises[1];
            DTOExercise exercise133 = day13.Exercises[2];

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

            DTOCycle cycle2 = DTOTestUserInformation.Cycles[1];
            DTODay day21 = cycle2.Days[0];
            DTOExercise exercise211 = day21.Exercises[0];
            DTOExercise exercise212 = day21.Exercises[1];
            DTOExercise exercise213 = day21.Exercises[2];

            DTODay day22 = cycle2.Days[1];
            DTOExercise exercise221 = day22.Exercises[0];
            DTOExercise exercise222 = day22.Exercises[1];
            DTOExercise exercise223 = day22.Exercises[2];

            DTODay day23 = cycle2.Days[2];
            DTOExercise exercise231 = day23.Exercises[0];
            DTOExercise exercise232 = day23.Exercises[1];
            DTOExercise exercise233 = day23.Exercises[2];

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

            DTOCycle cycle3 = DTOTestUserInformation.Cycles[2];
            DTODay day31 = cycle3.Days[0];
            DTOExercise exercise311 = day31.Exercises[0];
            DTOExercise exercise312 = day31.Exercises[1];
            DTOExercise exercise313 = day31.Exercises[2];

            DTODay day32 = cycle3.Days[1];
            DTOExercise exercise321 = day32.Exercises[0];
            DTOExercise exercise322 = day32.Exercises[1];
            DTOExercise exercise323 = day32.Exercises[2];

            DTODay day33 = cycle3.Days[2];
            DTOExercise exercise331 = day33.Exercises[0];
            DTOExercise exercise332 = day33.Exercises[1];
            DTOExercise exercise333 = day33.Exercises[2];

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

        private DTOUserInformation GetDTOUserInformationWithCycles()
        {
            DTOUserInformation DTOUserInformation = new DTOUserInformation()
            {
                FirstName = "FirstName",
                Username = "Username",

                Cycles = new List<DTOCycle>()
                {
                    new DTOCycle()
                    {
                        Name = "Cycle1",
                        Days = new List<DTODay>()
                        {
                            new DTODay()
                            {
                                Name = "Day11",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 1,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise111",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 2,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise112",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 3,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise113",
                                    },

                                }
                            },
                            new DTODay()
                            {
                                Name = "Day12",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 4,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise121",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 5,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise122",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 6,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise123",
                                    },

                                }
                            },
                            new DTODay()
                            {
                                Name = "Day13",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 7,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise131",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 8,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise132",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 9,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise133",
                                    }
                                }
                            }
                        }
                    },

                    new DTOCycle()
                    {
                        Name = "Cycle2",
                        Days = new List<DTODay>()
                        {
                            new DTODay()
                            {
                                Name = "Day21",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 10,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise211",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 11,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise212",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 12,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise213",
                                    },

                                }
                            },
                            new DTODay()
                            {
                                Name = "Day22",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 13,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise221",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 14,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise222",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 15,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise223",
                                    },

                                }
                            },
                            new DTODay()
                            {
                                Name = "Day23",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 16,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise231",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 17,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise232",
                                    },
                                    new DTOExercise()
                                    {
                                        Id= 18,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise233",
                                    }
                                }
                            }
                        }
                    },

                    new DTOCycle()
                    {
                        Name = "Cycle3",
                        Days = new List<DTODay>()
                        {
                            new DTODay()
                            {
                                Name = "Day31",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 19,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise311",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 20,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise312",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 21,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise313",
                                    },

                                }
                            },
                            new DTODay()
                            {
                                Name = "Day32",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 22,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise321",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 23,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise322",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 24,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise323",
                                    },

                                }
                            },
                            new DTODay()
                            {
                                Name = "Day33",
                                Exercises = new List<DTOExercise>()
                                {
                                    new DTOExercise()
                                    {
                                        Id = 25,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise331",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 26,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise332",
                                    },
                                    new DTOExercise()
                                    {
                                        Id = 27,
                                        Mode = ExercisesMods.Count,
                                        Name = "Exercise333",
                                    }
                                }
                            }
                        }
                    },
                }
            };

            return DTOUserInformation;
        }

        private IQueryable<ResultExercise> GetResultExercises()
        {
            IQueryable<ResultExercise> results = new List<ResultExercise>()
            {
                new ResultExercise()
                {
                    ExerciseId = 1,
                    DateTime = DateTime.Now,
                    Count = 10,
                },

                new ResultExercise()
                {
                    ExerciseId = 1,
                    DateTime = DateTime.Now,
                    Count = 20,
                },

                new ResultExercise()
                {
                    ExerciseId = 1,
                    DateTime = DateTime.Now,
                    Count = 30,
                },

                new ResultExercise()
                {
                    ExerciseId = 2,
                    DateTime = DateTime.Now,
                    Count = 40,
                },
                new ResultExercise()
                {
                    ExerciseId = 2,
                    DateTime = DateTime.Now,
                    Count = 50,
                },
                new ResultExercise()
                {
                    ExerciseId = 2,
                    DateTime = DateTime.Now,
                    Count = 60,
                },

                new ResultExercise()
                {
                    ExerciseId = 3,
                    DateTime = DateTime.Now,
                    Count = 70,
                },
                new ResultExercise()
                {
                    ExerciseId = 3,
                    DateTime = DateTime.Now,
                    Count = 80,
                },
                new ResultExercise()
                {
                    ExerciseId = 3,
                    DateTime = DateTime.Now,
                    Count = 90,
                },

                new ResultExercise()
                {
                    ExerciseId = 4,
                    DateTime = DateTime.Now,
                    Count = 100,
                },

                new ResultExercise()
                {
                    ExerciseId = 4,
                    DateTime = DateTime.Now,
                    Count = 110,
                },

                new ResultExercise()
                {
                    ExerciseId = 4,
                    DateTime = DateTime.Now,
                    Count = 120,
                },

                new ResultExercise()
                {
                    ExerciseId = 5,
                    DateTime = DateTime.Now,
                    Count = 130,
                },
                new ResultExercise()
                {
                    ExerciseId = 5,
                    DateTime = DateTime.Now,
                    Count = 140,
                },
                new ResultExercise()
                {
                    ExerciseId = 5,
                    DateTime = DateTime.Now,
                    Count = 150,
                },

                new ResultExercise()
                {
                    ExerciseId = 6,
                    DateTime = DateTime.Now,
                    Count = 160,
                },
                new ResultExercise()
                {
                    ExerciseId = 6,
                    DateTime = DateTime.Now,
                    Count = 170,
                },
                new ResultExercise()
                {
                    ExerciseId = 6,
                    DateTime = DateTime.Now,
                    Count = 180,
                },

                new ResultExercise()
                {
                    ExerciseId = 7,
                    DateTime = DateTime.Now,
                    Count = 190,
                },

                new ResultExercise()
                {
                    ExerciseId = 7,
                    DateTime = DateTime.Now,
                    Count = 200,
                },

                new ResultExercise()
                {
                    ExerciseId = 7,
                    DateTime = DateTime.Now,
                    Count = 210,
                },

                new ResultExercise()
                {
                    ExerciseId = 8,
                    DateTime = DateTime.Now,
                    Count = 220,
                },

                new ResultExercise()
                {
                    ExerciseId = 8,
                    DateTime = DateTime.Now,
                    Count = 230,
                },

                new ResultExercise()
                {
                    ExerciseId = 8,
                    DateTime = DateTime.Now,
                    Count = 240,
                },

                new ResultExercise()
                {
                    ExerciseId = 9,
                    DateTime = DateTime.Now,
                    Count = 250,
                },

                new ResultExercise()
                {
                    ExerciseId = 9,
                    DateTime = DateTime.Now,
                    Count = 260,
                },

                new ResultExercise()
                {
                    ExerciseId = 9,
                    DateTime = DateTime.Now,
                    Count = 270,
                },

                new ResultExercise()
                {
                    ExerciseId = 10,
                    DateTime = DateTime.Now,
                    Count = 280,
                },

                new ResultExercise()
                {
                    ExerciseId = 10,
                    DateTime = DateTime.Now,
                    Count = 290,
                },

                new ResultExercise()
                {
                    ExerciseId = 10,
                    DateTime = DateTime.Now,
                    Count = 300,
                },

                new ResultExercise()
                {
                    ExerciseId = 11,
                    DateTime = DateTime.Now,
                    Count = 310,
                },

                new ResultExercise()
                {
                    ExerciseId = 11,
                    DateTime = DateTime.Now,
                    Count = 320,
                },

                new ResultExercise()
                {
                    ExerciseId = 11,
                    DateTime = DateTime.Now,
                    Count = 330,
                },

                new ResultExercise()
                {
                    ExerciseId = 12,
                    DateTime = DateTime.Now,
                    Count = 340,
                },
                new ResultExercise()
                {
                    ExerciseId = 12,
                    DateTime = DateTime.Now,
                    Count = 350,
                },
                new ResultExercise()
                {
                    ExerciseId = 12,
                    DateTime = DateTime.Now,
                    Count = 360,
                },

                new ResultExercise()
                {
                    ExerciseId = 13,
                    DateTime = DateTime.Now,
                    Count = 370,
                },

                new ResultExercise()
                {
                    ExerciseId = 13,
                    DateTime = DateTime.Now,
                    Count = 380,
                },

                new ResultExercise()
                {
                    ExerciseId = 13,
                    DateTime = DateTime.Now,
                    Count = 390,
                },

                new ResultExercise()
                {
                    ExerciseId = 14,
                    DateTime = DateTime.Now,
                    Count = 400,
                },
                new ResultExercise()
                {
                    ExerciseId = 14,
                    DateTime = DateTime.Now,
                    Count = 410,
                },
                new ResultExercise()
                {
                    ExerciseId = 14,
                    DateTime = DateTime.Now,
                    Count = 420,
                },

                new ResultExercise()
                {
                    ExerciseId = 15,
                    DateTime = DateTime.Now,
                    Count = 430,
                },
                new ResultExercise()
                {
                    ExerciseId = 15,
                    DateTime = DateTime.Now,
                    Count = 440,
                },
                new ResultExercise()
                {
                    ExerciseId = 15,
                    DateTime = DateTime.Now,
                    Count = 450,
                },

                new ResultExercise()
                {
                    ExerciseId = 16,
                    DateTime = DateTime.Now,
                    Count = 460,
                },

                new ResultExercise()
                {
                    ExerciseId = 16,
                    DateTime = DateTime.Now,
                    Count = 470,
                },

                new ResultExercise()
                {
                    ExerciseId = 16,
                    DateTime = DateTime.Now,
                    Count = 480,
                },

                new ResultExercise()
                {
                    ExerciseId = 17,
                    DateTime = DateTime.Now,
                    Count = 490,
                },

                new ResultExercise()
                {
                    ExerciseId = 17,
                    DateTime = DateTime.Now,
                    Count = 500,
                },

                new ResultExercise()
                {
                    ExerciseId = 17,
                    DateTime = DateTime.Now,
                    Count = 510,
                },

                new ResultExercise()
                {
                    ExerciseId = 18,
                    DateTime = DateTime.Now,
                    Count = 520,
                },

                new ResultExercise()
                {
                    ExerciseId = 18,
                    DateTime = DateTime.Now,
                    Count = 530,
                },

                new ResultExercise()
                {
                    ExerciseId = 18,
                    DateTime = DateTime.Now,
                    Count = 540,
                },

                new ResultExercise()
                {
                    ExerciseId = 19,
                    DateTime = new DateTime(2020, 4, 1),
                    Count = 550,
                },

                new ResultExercise()
                {
                    ExerciseId = 19,
                    DateTime = new DateTime(2020, 4, 2),
                    Count = 560,
                },

                new ResultExercise()
                {
                    ExerciseId = 19,
                    DateTime = new DateTime(2020, 4, 3),
                    Count = 570,
                },

                new ResultExercise()
                {
                    ExerciseId = 20,
                    DateTime = new DateTime(2020, 5, 1),
                    Count = 580,
                },

                new ResultExercise()
                {
                    ExerciseId = 20,
                    DateTime = new DateTime(2020, 5, 2),
                    Count = 590,
                },

                new ResultExercise()
                {
                    ExerciseId = 20,
                    DateTime = new DateTime(2020, 5, 3),
                    Count = 600,
                },

                new ResultExercise()
                {
                    ExerciseId = 21,
                    DateTime = new DateTime(2020, 6, 1),
                    Count = 610,
                },
                new ResultExercise()
                {
                    ExerciseId = 21,
                    DateTime = new DateTime(2020, 6, 2),
                    Count = 620,
                },
                new ResultExercise()
                {
                    ExerciseId = 21,
                    DateTime = new DateTime(2020, 6, 3),
                    Count = 630,
                },

                new ResultExercise()
                {
                    ExerciseId = 22,
                    DateTime = new DateTime(2020, 7, 1),
                    Count = 640,
                },

                new ResultExercise()
                {
                    ExerciseId = 22,
                    DateTime = new DateTime(2020, 7, 2),
                    Count = 650,
                },

                new ResultExercise()
                {
                    ExerciseId = 22,
                    DateTime = new DateTime(2020, 7, 3),
                    Count = 660,
                },

                new ResultExercise()
                {
                    ExerciseId = 23,
                    DateTime = new DateTime(2020, 8, 1),
                    Count = 670,
                },
                new ResultExercise()
                {
                    ExerciseId = 23,
                    DateTime = new DateTime(2020, 8, 2),
                    Count = 680,
                },
                new ResultExercise()
                {
                    ExerciseId = 23,
                    DateTime = new DateTime(2020, 8, 3),
                    Count = 690,
                },

                new ResultExercise()
                {
                    ExerciseId = 24,
                    DateTime = new DateTime(2020, 9, 1),
                    Count = 700,
                },
                new ResultExercise()
                {
                    ExerciseId = 24,
                    DateTime = new DateTime(2020, 9, 2),
                    Count = 710,
                },
                new ResultExercise()
                {
                    ExerciseId = 24,
                    DateTime = new DateTime(2020, 9, 3),
                    Count = 720,
                },

                new ResultExercise()
                {
                    ExerciseId = 25,
                    DateTime = new DateTime(2020, 10, 1),
                    Count = 730,
                },

                new ResultExercise()
                {
                    ExerciseId = 25,
                    DateTime = new DateTime(2020, 10, 2),
                    Count = 740,
                },

                new ResultExercise()
                {
                    ExerciseId = 25,
                    DateTime = new DateTime(2020, 10, 3),
                    Count = 750,
                },

                new ResultExercise()
                {
                    ExerciseId = 26,
                    DateTime = new DateTime(2020, 11, 1),
                    Count = 760,
                },

                new ResultExercise()
                {
                    ExerciseId = 26,
                    DateTime = new DateTime(2020, 11, 2),
                    Count = 770,
                },

                new ResultExercise()
                {
                    ExerciseId = 26,
                    DateTime = new DateTime(2020, 11, 3),
                    Count = 780,
                },

                new ResultExercise()
                {
                    ExerciseId = 27,
                    DateTime = new DateTime(2020, 12, 1),
                    Count = 790,
                },

                new ResultExercise()
                {
                    ExerciseId = 27,
                    DateTime = new DateTime(2020, 12, 2),
                    Count = 800,
                },

                new ResultExercise()
                {
                    ExerciseId = 27,
                    DateTime = new DateTime(2020, 12, 3),
                    Count = 810,
                },

            }.AsQueryable();

            return results;
        }
    }
}