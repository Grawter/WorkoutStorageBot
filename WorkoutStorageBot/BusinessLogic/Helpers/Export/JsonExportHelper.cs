using Microsoft.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Export
{
    internal static class JsonExportHelper
    {
        private static JsonSerializerOptions starterOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // игнорирования сериализации для null свойств
            ReferenceHandler = ReferenceHandler.IgnoreCycles, // игнорировать зацикленность
        };

        private static JsonSerializerOptions finalOptions = new JsonSerializerOptions(starterOptions)
        {
            WriteIndented = true, // форматированная сериализация с табуляцией
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // Чтобы кириллические (или возможные другие) символы не экранировались
        };

        internal static async Task<RecyclableMemoryStream> GetJSONFile(List<DTOCycle> allUserCycles, IQueryable<ResultExercise> allUserResultsExercises, int monthFilterPeriod)
        {
            UnionExercisesAndResults(allUserCycles, allUserResultsExercises, monthFilterPeriod);

            JsonNode rootNode = await GetFinalRootNode(allUserCycles);

            RecyclableMemoryStream recyclableMemoryStream = CommonExportHelper.RecyclableMSManager.GetStream();
            await JsonSerializer.SerializeAsync(recyclableMemoryStream, rootNode, finalOptions);

            return recyclableMemoryStream;
        }

        private static async Task<JsonNode> GetFinalRootNode(List<DTOCycle> allUserCycles)
        {
            using RecyclableMemoryStream tempStream = CommonExportHelper.RecyclableMSManager.GetStream();
            
            await JsonSerializer.SerializeAsync(tempStream, allUserCycles, starterOptions);
            tempStream.Position = 0;

            JsonNode rootNode = await JsonNode.ParseAsync(tempStream)
                ?? throw new InvalidOperationException("Не удалось получить json rootNode");
            
            RemoveAdminInfo(rootNode);

            return rootNode;
        }

        private static void UnionExercisesAndResults(List<DTOCycle> allUserCycles, IQueryable<ResultExercise> allUserResultsExercises, int monthFilterPeriod)
        {
            ArgumentNullException.ThrowIfNull(allUserCycles);
            ArgumentNullException.ThrowIfNull(allUserResultsExercises);

            DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, allUserResultsExercises);
            IQueryable<ResultExercise> resultExercisesByFilterData = CommonExportHelper.GetResultExercisesByFilterDate(allUserResultsExercises, filterDateTime);

            IEnumerable<DTOExercise> allUserExercises = allUserCycles.SelectMany(x => x.Days).SelectMany(y => y.Exercises);

            foreach (DTOExercise userExercise in allUserExercises)
            {
                userExercise.ResultsExercise = resultExercisesByFilterData.Where(x => x.ExerciseId == userExercise.Id)
                                                                          .Select(y => EntityConverter.ToDTOResultExercise(y))
                                                                          .ToList();
            }
        }

        private static void RemoveAdminInfo(JsonNode rootNode)
        {
            if (rootNode is JsonArray array)
            {
                foreach (JsonNode subRootNode in array)
                {
                    if (subRootNode is JsonObject obj)
                    {
                        obj.Remove("UserInformationId");
                        obj.Remove("UserInformation");
                        obj.Remove("IsActive");
                        obj.Remove("IsArchive");
                    }
                    else
                        throw new InvalidOperationException("Не удалось корректно обработать json (Ожидался JsonObject).");
                }
            }
            else
                throw new InvalidOperationException("Не удалось корректно обработать json (Ожидался JsonArray).");

            return;
        }
    }
}