#region using

using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using WorkoutStorageBot.Model.Domain;

#endregion

namespace WorkoutStorageBot.Helpers.Export
{
    internal static class ExcelExportHelper
    {
        internal static byte[] GetExcelFile(List<Cycle> cycles, IQueryable<ResultExercise> resultsExercises, int monthFilterPeriod)
        {
            SetLicense();

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

                DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, resultsExercises);
                CommonExportHelper.LoadDBDataToDBContextForFilterDate(resultsExercises, filterDateTime);

                string informationAboutSelectedPeriod = filterDateTime > DateTime.MinValue
                                ? $"Временной промежуток формирования данных {filterDateTime.ToShortDateString()} - {filterDateTime.AddMonths(monthFilterPeriod).ToShortDateString()}"
                                : "Тренировки, зафиксированные за всё время";            

                // informationAboutSelectedPeriod
                SetStyle(mainSheet.Cells[informationAboutSelectedPeriodPoint.X, informationAboutSelectedPeriodPoint.Y, 
                                         informationAboutSelectedPeriodPoint.X, informationAboutSelectedPeriodPoint.Y + 7],
                                         true, true, false, Color.Azure);
                mainSheet.Cells[informationAboutSelectedPeriodPoint.X, informationAboutSelectedPeriodPoint.Y].Value = informationAboutSelectedPeriod;

                foreach (Cycle cycle in cycles)
                {
                    foreach (Day day in cycle.Days)
                    {
                        foreach (Exercise exercise in day.Exercises)
                        {
                            // style for ExerciseName
                            SetStyle(mainSheet.Cells[exercisePoint.X, exercisePoint.Y, exercisePoint.X, exercisePoint.Y + 3], true, true, false, Color.NavajoWhite);
                            mainSheet.Cells[exercisePoint.X, exercisePoint.Y].Value = exercise.Name;

                            // ResultTitle: Date, Count, Weight, FreeResult 
                            SetStyle(mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y], false, true, false, Color.Salmon);
                            mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y].Value = "Дата";
                            SetStyle(mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 1], false, true, false, Color.SkyBlue);
                            mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 1].Value = "Кол-во";
                            SetStyle(mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 2], false, true, false, Color.DarkSeaGreen);
                            mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 2].Value = "Вес";
                            SetStyle(mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 3], false, true, false, Color.LightCyan);
                            mainSheet.Cells[resultTitlePoint.X, resultTitlePoint.Y + 3].Value = "Свободный рез.";

                            foreach (ResultExercise resultExercise in exercise.ResultExercises)
                            {
                                Color backgroundCellColor = GetColorForRow(++rowNumber);

                                SetStyle(mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y], false, true, false, Color.Yellow);
                                mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y].Value = resultExercise.DateTime.ToShortDateString();
                                SetStyle(mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 1], false, true, false, backgroundCellColor);
                                mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 1].Value = resultExercise.Count;
                                SetStyle(mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 2], false, true, false, backgroundCellColor);
                                mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 2].Value = resultExercise.Weight;
                                SetStyle(mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 3], false, true, true, backgroundCellColor);
                                mainSheet.Cells[resultExercisePoint.X, resultExercisePoint.Y + 3].Value = resultExercise.FreeResult;

                                // // shift to next resultExercisePoint
                                resultExercisePoint.X += 1;
                            }

                            // // shift to next exercisePoint, resultTitlePoint and resultExercisePoint
                            rowNumber = 0;

                            exercisePoint.Y += 4;

                            resultTitlePoint.X = exercisePoint.X + 1;
                            resultTitlePoint.Y = exercisePoint.Y;

                            resultExercisePoint.X = resultTitlePoint.X + 1;
                            resultExercisePoint.Y = resultTitlePoint.Y;

                        }

                        // style for DayName
                        SetStyle(mainSheet.Cells[dayPoint.X, dayPoint.Y, dayPoint.X, exercisePoint.Y - 1], true, true, false, Color.Gold);
                        mainSheet.Cells[dayPoint.X, dayPoint.Y].Value = day.Name;

                        // shift to next day
                        dayPoint.Y = exercisePoint.Y;
                    }

                    // style for CycleName
                    SetStyle(mainSheet.Cells[cyclePoint.X, cyclePoint.Y, cyclePoint.X, exercisePoint.Y - 1], true, true, false, Color.PaleGreen);
                    mainSheet.Cells[cyclePoint.X, cyclePoint.Y].Value = cycle.Name;

                    // shift to next cycle
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

        private static void SetLicense()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            /*
             * Для версий EPPlus 8 и выше. 
             * Ставит текст в теги и комментарии к документу, что сделано при помощи EPPlus по некоммерческой лицензии
             * для такого то проекта (указывается строка в аргументе).
             * В целом, совсем не мешает, но пускай остаётся старая версия "без" этих доп. надписей.
            */ 
            //ExcelPackage.License.SetNonCommercialPersonal("WorkoutStorageBot");
        }

        private static void SetStyle(ExcelRange excelRange, 
            bool needMerge, bool needBorder, bool wrapText, 
            Color backgroundCellColor, 
            ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Center, 
            ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Center)
        {
            excelRange.Merge = needMerge;              
           
            if (needBorder)
                excelRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            excelRange.Style.VerticalAlignment = verticalAlignment;
            excelRange.Style.HorizontalAlignment = horizontalAlignment;
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(backgroundCellColor);
            excelRange.Style.WrapText = wrapText;
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