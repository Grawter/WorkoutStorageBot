using Microsoft.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Export
{
    internal static class ExcelExportHelper
    {
        internal static async Task<RecyclableMemoryStream> GetExcelFile(List<DTOCycle> allUserCycles, IQueryable<ResultExercise> allUserResultsExercises, int monthFilterPeriod)
        {
            SetLicense();

            DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, allUserResultsExercises);
            IQueryable<ResultExercise> resultExercisesByFilterData = CommonExportHelper.GetResultExercisesByFilterDate(allUserResultsExercises, filterDateTime);

            RecyclableMemoryStream recyclableMemoryStream = CommonExportHelper.RecyclableMSManager.GetStream();

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
                                ? $"Временной промежуток формирования данных {filterDateTime.ToString(CommonConsts.Common.DateFormat)} - {filterDateTime.AddMonths(monthFilterPeriod).ToString(CommonConsts.Common.DateFormat)}"
                                : "Тренировки, зафиксированные за всё время";

                // informationAboutSelectedPeriod
                SetStyle(mainSheet.Cells[informationAboutSelectedPeriodPoint.Y, informationAboutSelectedPeriodPoint.X,
                                         informationAboutSelectedPeriodPoint.Y, informationAboutSelectedPeriodPoint.X + 7],
                                         needMerge: true,
                                         needBorder: true,
                                         backgroundCellColor: Color.Azure);
                mainSheet.Cells[informationAboutSelectedPeriodPoint.Y, informationAboutSelectedPeriodPoint.X].Value = informationAboutSelectedPeriod;

                int resultExerciseRowNumber = 0;

                foreach (DTOCycle cycle in allUserCycles)
                {
                    foreach (DTODay day in cycle.Days)
                    {
                        foreach (DTOExercise exercise in day.Exercises)
                        {
                            SetExerciseName(exercise.Name, exercisePoint, mainSheet);

                            SetResultTitle(resultTitlePoint, mainSheet);

                            DateTime tempDate = DateTime.MinValue;

                            exercise.ResultsExercise = resultExercisesByFilterData.Where(x => x.ExerciseId == exercise.Id)
                                                                                  .Select(y => y.ToDTOResultExercise())
                                                                                  .ToList();

                            foreach (DTOResultExercise resultExercise in exercise.ResultsExercise)
                            {
                                bool isNeedWriteDate = false;
                                if (tempDate.Date != resultExercise.DateTime.Date)
                                {
                                    tempDate = resultExercise.DateTime.Date;
                                    isNeedWriteDate = true;
                                }

                                SetResultExercise(++resultExerciseRowNumber, resultExercise, resultExercisePoint, mainSheet, isNeedWriteDate);

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

                await package.SaveAsAsync(recyclableMemoryStream);

                return recyclableMemoryStream;
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

            SetStyle(mainSheet.Cells[exercisePoint.Y, exercisePoint.X, endExercisePoint.Y, endExercisePoint.X], needMerge: true, needBorder: true, backgroundCellColor: Color.NavajoWhite);
            mainSheet.Cells[exercisePoint.Y, exercisePoint.X].Value = exerciseName;
        }

        private static void SetResultTitle(Point resultTitlePoint, ExcelWorksheet mainSheet)
        {
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], needBorder: true, backgroundCellColor: Color.Salmon);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Дата";

            resultTitlePoint.X += 1;
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], needBorder: true, backgroundCellColor: Color.SkyBlue);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Кол-во";

            resultTitlePoint.X += 1;
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], needBorder: true, backgroundCellColor: Color.DarkSeaGreen);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Вес";

            resultTitlePoint.X += 1;
            SetStyle(mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X], needBorder: true, backgroundCellColor: Color.LightCyan);
            mainSheet.Cells[resultTitlePoint.Y, resultTitlePoint.X].Value = "Свободный рез.";
        }

        private static void SetResultExercise(int rowNumber, DTOResultExercise resultExercise, Point resultExercisePoint, ExcelWorksheet mainSheet, bool isNeedWriteDate = true)
        {
            Color backgroundCellColor = GetColorForRow(rowNumber);

            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], needBorder: true, needBold: true, backgroundCellColor: Color.Yellow);
            if (isNeedWriteDate)
                mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.DateTime.ToString(CommonConsts.Common.DateFormat);

            resultExercisePoint.X += 1;
            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], needBorder: true, backgroundCellColor: backgroundCellColor);
            mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.Count;

            resultExercisePoint.X += 1;
            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], needBorder: true, backgroundCellColor: backgroundCellColor);
            mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.Weight;

            resultExercisePoint.X += 1;
            SetStyle(mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X], needBorder: true, needWrapText: true, backgroundCellColor: backgroundCellColor);
            mainSheet.Cells[resultExercisePoint.Y, resultExercisePoint.X].Value = resultExercise.FreeResult;
        }

        private static void SetDayName(string dayName, Point startDayPoint, Point endDayPoint, ExcelWorksheet mainSheet)
        {
            SetStyle(mainSheet.Cells[startDayPoint.Y, startDayPoint.X, endDayPoint.Y, endDayPoint.X], needMerge: true, needBorder: true, backgroundCellColor: Color.Gold);
            mainSheet.Cells[startDayPoint.Y, startDayPoint.X].Value = dayName;
        }

        private static void SetCycleName(string cycleName, Point startCyclePoint, Point endCyclePoint, ExcelWorksheet mainSheet)
        {
            SetStyle(mainSheet.Cells[startCyclePoint.Y, startCyclePoint.X, endCyclePoint.Y, endCyclePoint.X], needMerge: true, needBorder: true, backgroundCellColor: Color.PaleGreen);
            mainSheet.Cells[startCyclePoint.Y, startCyclePoint.X].Value = cycleName;
        }

        private static void SetStyle(ExcelRange excelRange,
                                     bool needMerge = false,
                                     bool needBorder = false,
                                     bool needWrapText = false,
                                     bool needBold = false,
                                     Color backgroundCellColor = default,
                                     ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Center,
                                     ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Center)
        {
            if (backgroundCellColor == default)
                backgroundCellColor = Color.White;

            excelRange.Merge = needMerge;

            if (needBorder)
                excelRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            excelRange.Style.VerticalAlignment = verticalAlignment;
            excelRange.Style.HorizontalAlignment = horizontalAlignment;
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(backgroundCellColor);
            excelRange.Style.WrapText = needWrapText;
            excelRange.Style.Font.Bold = needBold;
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