#region using

using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using WorkoutStorageBot.Model.DomainsAndEntities;

#endregion

namespace WorkoutStorageBot.Helpers.Export
{
    internal static class ExcelExportHelper
    {
        internal static byte[] GetExcelFile(List<Cycle> cycles, IQueryable<ResultExercise> resultsExercises, int monthFilterPeriod)
        {
            SetLicense();

            DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, resultsExercises);
            CommonExportHelper.LoadDBDataToDBContextForFilterDate(resultsExercises, filterDateTime);

            byte[] packageInfo;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet mainSheet = package.Workbook.Worksheets.Add("Workout");

                int startPosition = 1;

                // init start positions
                Point informationAboutSelectedPeriodPoint = new Point(startPosition, 2);
                
                Point startCyclePoint = new Point(startPosition, 4);
                Point endCyclePoint = new Point(startPosition, 4);
                Point startDayPoint = new Point(startPosition, startCyclePoint.Y + 1);
                Point endDayPoint = new Point(startPosition, startCyclePoint.Y + 1);
                Point exercisePoint = new Point(startPosition, startDayPoint.Y + 1);
                Point resultTitlePoint = new Point(startPosition, exercisePoint.Y + 1);
                Point resultExercisePoint = new Point(startPosition, resultTitlePoint.Y + 1);

                int maxResultExercisePointY = resultExercisePoint.Y;

                int newCyclePointY = 0;

                string informationAboutSelectedPeriod = filterDateTime > DateTime.MinValue
                                ? $"Временной промежуток формирования данных {filterDateTime.ToShortDateString()} - {filterDateTime.AddMonths(monthFilterPeriod).ToShortDateString()}"
                                : "Тренировки, зафиксированные за всё время";

                // informationAboutSelectedPeriod
                SetStyle(mainSheet.Cells[informationAboutSelectedPeriodPoint.Y, informationAboutSelectedPeriodPoint.X, 
                                         informationAboutSelectedPeriodPoint.Y, informationAboutSelectedPeriodPoint.X + 7],
                                         true, true, false, Color.Azure);
                mainSheet.Cells[informationAboutSelectedPeriodPoint.Y, informationAboutSelectedPeriodPoint.X].Value = informationAboutSelectedPeriod;

                int resultExerciseRowNumber = 0;

                foreach (Cycle cycle in cycles)
                {
                    foreach (Day day in cycle.Days)
                    {
                        foreach (Exercise exercise in day.Exercises)
                        {
                            SetExerciseName(exercise.Name, exercisePoint, mainSheet);

                            SetResultTitle(resultTitlePoint, mainSheet);

                            foreach (ResultExercise resultExercise in exercise.ResultExercises)
                            {
                                SetResultExercise(++resultExerciseRowNumber, resultExercise, resultExercisePoint, mainSheet);

                                // shift to next (row) resultExercisePoint
                                resultExercisePoint.X = exercisePoint.X;
                                resultExercisePoint.Y += 1;
                            }

                            resultExerciseRowNumber = 0;

                            if (resultExercisePoint.Y > maxResultExercisePointY)
                                maxResultExercisePointY = resultExercisePoint.Y;

                            // shift to next exercisePoint,
                            exercisePoint.X += 4;

                            // shift to next resultTitlePoint
                            resultTitlePoint.X = exercisePoint.X;

                            // shift to next resultExercisePoint
                            resultExercisePoint.X = resultTitlePoint.X;
                            resultExercisePoint.Y = resultTitlePoint.Y + 1;
                        }

                        // calculate endDayPoint
                        if (day.Exercises.Count == 0)
                            endDayPoint = new Point(startDayPoint.X + 1, startDayPoint.Y);
                        else
                            endDayPoint = new Point(exercisePoint.X - 1, startDayPoint.Y);

                        SetDayName(day.Name, startDayPoint, endDayPoint, mainSheet);

                        // shift to next day
                        startDayPoint.X = endDayPoint.X + 1;
                    }

                    // calculate endCyclePoint
                    endCyclePoint = new Point(endDayPoint.X, startCyclePoint.Y);
                    SetCycleName(cycle.Name, startCyclePoint, endCyclePoint, mainSheet);

                    // shift to next cycle
                    newCyclePointY = maxResultExercisePointY + 3;
                    if (newCyclePointY <= endCyclePoint.Y) // case ResultExercise hasnt elements 
                        newCyclePointY = endCyclePoint.Y + 3;

                    startCyclePoint = new Point(startPosition, newCyclePointY);
                    startDayPoint = new Point(startPosition, startCyclePoint.Y + 1);
                    exercisePoint = new Point(startPosition, startDayPoint.Y + 1);
                    resultTitlePoint = new Point(startPosition, exercisePoint.Y + 1);
                    resultExercisePoint = new Point(startPosition, resultTitlePoint.Y + 1);
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

        private static void SetExerciseName(string exerciseName, Point exercisePoint, ExcelWorksheet mainSheet)
        {
            Point endExercisePoint = new Point(exercisePoint.X + 3, exercisePoint.Y);

            SetStyle(mainSheet.Cells[exercisePoint.Y, exercisePoint.X, endExercisePoint.Y, endExercisePoint.X], true, true, false, Color.NavajoWhite);
            mainSheet.Cells[exercisePoint.Y, exercisePoint.X].Value = exerciseName;
        }

        private static void SetResultTitle(Point resultTitlePoint, ExcelWorksheet mainSheet)
        {
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], false, true, false, Color.Salmon);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Дата";

            resultTitlePoint.X += 1;
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], false, true, false, Color.SkyBlue);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Кол-во";

            resultTitlePoint.X += 1;
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], false, true, false, Color.DarkSeaGreen);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Вес";

            resultTitlePoint.X += 1;
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], false, true, false, Color.LightCyan);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Свободный рез.";
        }

        private static void SetResultExercise(int rowNumber, ResultExercise resultExercise, Point resultExercisePoint, ExcelWorksheet mainSheet)
        {
            Color backgroundCellColor = GetColorForRow(rowNumber);

            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], false, true, false, Color.Yellow);
            mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.DateTime.ToShortDateString();

            resultExercisePoint.X += 1;
            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], false, true, false, backgroundCellColor);
            mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.Count;

            resultExercisePoint.X += 1;
            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], false, true, false, backgroundCellColor);
            mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.Weight;

            resultExercisePoint.X += 1;
            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], false, true, true, backgroundCellColor);
            mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.FreeResult;
        }

        private static void SetDayName(string dayName, Point startDayPoint, Point endDayPoint, ExcelWorksheet mainSheet)
        {
            SetStyle(mainSheet.Cells[startDayPoint.Y, startDayPoint.X, endDayPoint.Y, endDayPoint.X], true, true, false, Color.Gold);
            mainSheet.Cells[startDayPoint.Y, startDayPoint.X].Value = dayName;
        }

        private static void SetCycleName(string cycleName, Point startCyclePoint, Point endCyclePoint, ExcelWorksheet mainSheet)
        {
            SetStyle(mainSheet.Cells[startCyclePoint.Y, startCyclePoint.X, endCyclePoint.Y, endCyclePoint.X], true, true, false, Color.PaleGreen);
            mainSheet.Cells[startCyclePoint.Y, startCyclePoint.X].Value = cycleName;
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