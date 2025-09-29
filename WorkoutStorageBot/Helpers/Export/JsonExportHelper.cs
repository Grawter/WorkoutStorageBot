#region using

using Microsoft.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WorkoutStorageBot.Model.DomainsAndEntities;

#endregion

namespace WorkoutStorageBot.Helpers.Export
{
    internal static class JsonExportHelper
    {
        internal static RecyclableMemoryStream GetJSONFile(List<Cycle> cycles, IQueryable<ResultExercise> resultsExercises, int monthFilterPeriod)
        {
            string json = GetJSONFileStr(cycles, resultsExercises, monthFilterPeriod);

            byte[] byteJson = new UTF8Encoding(true).GetBytes(json);

            RecyclableMemoryStream recyclableMemoryStream = CommonExportHelper.RecyclableMSManager.GetStream();

            using (StreamWriter writer = new StreamWriter(recyclableMemoryStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(json);
            }

            return recyclableMemoryStream;
        }

        internal static string GetJSONFileStr(List<Cycle> cycles, IQueryable<ResultExercise> resultsExercises, int monthFilterPeriod)
        {
            ArgumentNullException.ThrowIfNull(cycles);
            ArgumentNullException.ThrowIfNull(resultsExercises);

            DateTime filterDateTime = CommonExportHelper.GetFilterDateTime(monthFilterPeriod, resultsExercises);
            CommonExportHelper.LoadDBDataToDBContextForFilterDate(resultsExercises, filterDateTime);

            JsonSerializerOptions starterJsonSerializerOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            JsonNode rootNode = JsonNode.Parse(JsonSerializer.Serialize(cycles, starterJsonSerializerOptions))
                ?? throw new InvalidOperationException("Не удалось получить json rootNode");

            RemoveAdminInfo(rootNode);

            // Нельзя изменять уже используемую настройку (starterJsonSerializerOptions)
            JsonSerializerOptions finalJsonSerializerOptions = new JsonSerializerOptions(starterJsonSerializerOptions) 
            {
                WriteIndented = true, // форматированный сериалзиация с табуляцией
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