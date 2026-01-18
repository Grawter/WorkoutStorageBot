using Telegram.Bot.Types;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results
{
    internal class HandlerResult
    {
        internal required Update Update { get; init; }

        internal required bool HasAccess { get; set; }

        internal bool IsNeedContinue { get; set; }
    }
}