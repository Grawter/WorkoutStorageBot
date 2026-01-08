using Microsoft.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Entities.BusinessLogic;

namespace WorkoutStorageBot.Helpers.Export
{
    internal static class JsonExportHelper
    {
        internal static RecyclableMemoryStream GetJSONFile(List<DTOCycle> allUserCycles, IQueryable<ResultExercise> allUserResultsExercises, int monthFilterPeriod)
        {
            string json = GetJSONFileStr(allUserCycles, allUserResultsExercises, monthFilterPeriod);

            byte[] byteJson = new UTF8Encoding(true).GetBytes(json);

            RecyclableMemoryStream recyclableMemoryStream = CommonExportHelper.RecyclableMSManager.GetStream();

            using (StreamWriter writer = new StreamWriter(recyclableMemoryStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(json);
            }

            return recyclableMemoryStream;
        }

        internal static string GetJSONFileStr(List<DTOCycle> allUserCycles, IQueryable<ResultExercise> allUserResultsExercises, int monthFilterPeriod)
        {
            ArgumentNullException.ThrowIfNull(allUserCycles);
            ArgumentNullException.ThrowIfNull(allUserResultsExercises);

            DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, allUserResultsExercises);
            IQueryable<ResultExercise> resultExercisesByFilterData = CommonExportHelper.GetResultExercisesByFilterDate(allUserResultsExercises, filterDateTime);

            JsonSerializerOptions starterJsonSerializerOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            IEnumerable<DTOExercise> allUserExercises = allUserCycles.SelectMany(x => x.Days).SelectMany(y => y.Exercises);

            foreach (DTOExercise userExercise in allUserExercises)
            {
                userExercise.ResultsExercise = resultExercisesByFilterData.Where(x => x.ExerciseId == userExercise.Id)
                                                                          .Select(y => EntityConverter.ToDTOResultExercise(y))
                                                                          .ToList();
            }

            JsonNode rootNode = JsonNode.Parse(JsonSerializer.Serialize(allUserCycles, starterJsonSerializerOptions))
                ?? throw new InvalidOperationException("Не удалось получить json rootNode");

            RemoveAdminInfo(rootNode);

            // Нельзя изменять уже используемую настройку (starterJsonSerializerOptions)
            JsonSerializerOptions finalJsonSerializerOptions = new JsonSerializerOptions(starterJsonSerializerOptions) 
            {
                WriteIndented = true, // форматированная сериализация с табуляцией
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // Чтобы кириллические (или возможные другие) символы не экранировались
            };

            return rootNode.ToJsonString(finalJsonSerializerOptions);
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