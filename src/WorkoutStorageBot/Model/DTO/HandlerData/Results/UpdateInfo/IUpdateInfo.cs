using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo
{
    public interface IUpdateInfo
    {
        UpdateType UpdateType { get; }

        bool IsExpectedType { get; }

        Update Update { get; }
    }
}