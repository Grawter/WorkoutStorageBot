using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using WorkoutStorageBot.Model;

namespace WorkoutStorageBot.Helpers.Export
{
    internal static class ExcelExportHelper
    {
        internal static byte[] GetExcelFile(List<Cycle> cycles, IQueryable<ResultExercise> resultsExercises, int monthFilterPeriod)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            byte[] packageInfo;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet mainSheet = package.Workbook.Worksheets.Add("Workout");

                Point informationAboutSelectedPeriodPoint = new Point(2, 1);

                Point cyclePoint = new Point(4, 1);
                Point dayPoint = new Point(cyclePoint.X + 1, 1);
                Point exercisePoint = new Point(dayPoint.X + 1, 1);
                Point resultTitlePoint = new Point(exercisePoint.X + 1, 1);
                Point resultExercisePoint = new Point(resultTitlePoint.X + 1, 1);

                int rowNumber = 0;

                DateTime filterDateTime = default;

                string informationAboutSelectedPeriod;

                if (monthFilterPeriod > 0)
                {
                    filterDateTime = resultsExercises
                                                .Select(r => r.DateTime)
                                                .Max()
                                                .AddMonths(-monthFilterPeriod);

                    informationAboutSelectedPeriod = $"Временной промежуток формирования данных {filterDateTime.ToShortDateString()} - {DateTime.Now.ToShortDateString()}";
                }
                else
                    informationAboutSelectedPeriod = "Тренировки, зафиксированные за всё время";

                SetStyle(mainSheet.Cells[informationAboutSelectedPeriodPoint.X, informationAboutSelectedPeriodPoint.Y, 
                                         informationAboutSelectedPeriodPoint.X, informationAboutSelectedPeriodPoint.Y + 7],
                                         true, true, ExcelHorizontalAlignment.Center, Color.Azure);
                mainSheet.Cells[informationAboutSelectedPeriodPoint.X, informationAboutSelectedPeriodPoint.Y].Value = informationAboutSelectedPeriod;

                foreach (Cycle cycle in cycles)
                {
                    foreach (Day day in cycle.Days)
                    {
                        foreach (Exercise exercise in day.Exercises)
                        {
                            SetStyle(mainSheet.Cells[exercisePoint.X, exercisePoint.Y, exercisePoint.X, exercisePoint.Y + 2], true, true, ExcelHorizontalAlignment.Center, Color.NavajoWhite);
                            mainSheet.Cells[exercisePoint.X, exercisePoint.Y].Value = exercise.Name;

                            // ResultTitle: Date, Weight, Count
                            SetStyle(mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y], false, true, ExcelHorizontalAlignment.Center, Color.Salmon);
                            mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y].Value = "Дата";
                            SetStyle(mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 1], false, true, ExcelHorizontalAlignment.Center, Color.DarkSeaGreen);
                            mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 1].Value = "Вес";
                            SetStyle(mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 2], false, true, ExcelHorizontalAlignment.Center, Color.SkyBlue);
                            mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 2].Value = "Кол-во";

                            foreach (ResultExercise resultExercise in resultsExercises.Where(re => re.ExerciseId == exercise.Id &&
                                                                                             (monthFilterPeriod > 0 
                                                                                                   ? re.DateTime >= resultsExercises
                                                                                                                                .Select(r => r.DateTime)
                                                                                                                                .Max()
                                                                                                                                .AddMonths(-monthFilterPeriod)
                                                                                                   : true)))
                            {
                                Color backgroundCellColor = GetColorForRow(++rowNumber);

                                SetStyle(mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y], false, true, ExcelHorizontalAlignment.Center, Color.Yellow);
                                mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y].Value = resultExercise.DateTime.ToShortDateString();
                                SetStyle(mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 1], false, true, ExcelHorizontalAlignment.Center, backgroundCellColor);
                                mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 1].Value = resultExercise.Weight;
                                SetStyle(mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 2], false, true, ExcelHorizontalAlignment.Center, backgroundCellColor);
                                mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 2].Value = resultExercise.Count;

                                // // shift to next resultExercisePoint
                                resultExercisePoint.X += 1;
                            }

                            // // shift to next exercisePoint, resultTitlePoint and resultExercisePoint
                            rowNumber = 0;

                            exercisePoint.Y += 3;

                            resultTitlePoint.X = exercisePoint.X + 1;
                            resultTitlePoint.Y = exercisePoint.Y;

                            resultExercisePoint.X = resultTitlePoint.X + 1;
                            resultExercisePoint.Y = resultTitlePoint.Y;

                        }

                        SetStyle(mainSheet.Cells[dayPoint.X, dayPoint.Y, dayPoint.X, exercisePoint.Y - 1], true, true, ExcelHorizontalAlignment.Center, Color.Gold);
                        mainSheet.Cells[dayPoint.X, dayPoint.Y].Value = day.Name;

                        // shift to next day
                        dayPoint.Y = exercisePoint.Y;
                    }

                    SetStyle(mainSheet.Cells[cyclePoint.X, cyclePoint.Y, cyclePoint.X, exercisePoint.Y - 1], true, true, ExcelHorizontalAlignment.Center, Color.PaleGreen);
                    mainSheet.Cells[cyclePoint.X, cyclePoint.Y].Value = cycle.Name;

                    //// shift to next cycle
                    cyclePoint = new Point(resultExercisePoint.X + 3, 1);
                    dayPoint = new Point(cyclePoint.X + 1, 1);
                    exercisePoint = new Point(dayPoint.X + 1, 1);
                    resultTitlePoint = new Point(exercisePoint.X + 1, 1);
                    resultExercisePoint = new Point(resultTitlePoint.X + 1, 1);
                }

                mainSheet.Cells.AutoFitColumns();

                package.Save();

                return package.GetAsByteArray();
            }
        }

        private static void SetStyle(ExcelRange excelRange, bool needMerge, bool needBorder, ExcelHorizontalAlignment horizontalAlignment, Color backgroundCellColor)
        {
            excelRange.Merge = needMerge;              
           
            if (needBorder)
                excelRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            excelRange.Style.HorizontalAlignment = horizontalAlignment;
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(backgroundCellColor);
        }

        private static Color GetColorForRow(int rowNumber)
        {
            if (rowNumber % 2 == 0)
                return Color.Gainsboro;
            else
                return Color.White;
        }
    }
}