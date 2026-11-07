using Telegram.Bot.Types;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results
{
    internal class HandlerResult
    {
        internal required Update Update { get; init; }

        internal bool IsNeedStopBot { get; set; }

        internal bool IsNeedStopFurtherProcessing { get; set; }
    }
}