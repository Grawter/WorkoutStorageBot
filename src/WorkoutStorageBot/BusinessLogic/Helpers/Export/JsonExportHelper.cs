using Microsoft.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WorkoutStorageBot.BusinessLogic.Extenions;
using WorkoutStorageBot.BusinessLogic.Helpers.Stream;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageModels.Entities.BusinessLogic;

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

        internal static async Task<RecyclableMemoryStream> GetJSONFile(DTOUserInformation DTOUserInformation, IQueryable<ResultExercise> allUserResultsExercises, int monthFilterPeriod)
        {
            UnionExercisesAndResults(DTOUserInformation.Cycles, allUserResultsExercises, monthFilterPeriod);

            JsonNode rootNode = GetFinalRootNode(DTOUserInformation);

            RecyclableMemoryStream recyclableMemoryStream = StreamHelper.RecyclableMSManager.GetStream();
            await JsonSerializer.SerializeAsync(recyclableMemoryStream, rootNode, finalOptions);

            recyclableMemoryStream.Position = 0;
            return recyclableMemoryStream;
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
                                                                          .Select(y => y.ToDTOResultExercise())
                                                                          .ToList();
            }
        }

        private static JsonNode GetFinalRootNode(DTOUserInformation DTOUserInformation)
        {
            JsonNode rootNode = JsonSerializer.SerializeToNode(DTOUserInformation, starterOptions)
                ?? throw new InvalidOperationException("Не удалось получить json rootNode");
            
            RemoveSensitiveInfo(rootNode, ["UserId", "FirstName", "Username", "WhiteList", "BlackList"]);

            return rootNode;
        }

        private static void RemoveSensitiveInfo(JsonNode rootNode, List<string> propertyNamesToRemove)
        {
            JsonObject rootJsonObject = rootNode.AsObject();

            foreach (string propertyNameToRemove in propertyNamesToRemove)
            {
                rootJsonObject.Remove(propertyNameToRemove);
            }
        }
    }
}