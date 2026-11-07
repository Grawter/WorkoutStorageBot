using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WorkoutStorageBot.BusinessLogic.Extensions;
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

        internal static async Task<RecyclableMemoryStream> GetJSONFile(DTOUserInformation DTOUserInformation,
                                                                       IQueryable<ResultExercise> allUserResultsExercises,
                                                                       int monthFilterPeriod,
                                                                       CancellationToken cancellationToken)
        {
            await UnionExercisesAndResultsAsync(DTOUserInformation.Cycles, allUserResultsExercises, monthFilterPeriod, cancellationToken);

            JsonNode rootNode = GetFinalRootNode(DTOUserInformation);

            RecyclableMemoryStream recyclableMemoryStream = StreamHelper.RecyclableMSManager.GetStream();
            await JsonSerializer.SerializeAsync(recyclableMemoryStream, rootNode, finalOptions, cancellationToken);

            recyclableMemoryStream.Position = 0;
            return recyclableMemoryStream;
        }

        private static async Task UnionExercisesAndResultsAsync(List<DTOCycle> allUserCycles, IQueryable<ResultExercise> allUserResultsExercises, int monthFilterPeriod, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(allUserCycles);
            ArgumentNullException.ThrowIfNull(allUserResultsExercises);

            DateTime filterDateTime = await CommonExportHelper.GetFilterDateTime(monthFilterPeriod, allUserResultsExercises, cancellationToken);
            IQueryable<ResultExercise> resultExercisesByFilterData = CommonExportHelper.GetResultExercisesByFilterDate(allUserResultsExercises, filterDateTime);

            var allFilteredEntities = await resultExercisesByFilterData.ToListAsync(cancellationToken);

            var resultsLookup = allFilteredEntities.Select(y => y.ToDTOResultExercise())
                                                   .ToLookup(r => r.ExerciseId);

            IEnumerable<DTOExercise> allUserExercises = allUserCycles.SelectMany(x => x.Days).SelectMany(y => y.Exercises);

            foreach (DTOExercise userExercise in allUserExercises)
            {
                userExercise.ResultsExercise = resultsLookup[userExercise.Id].ToList();
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