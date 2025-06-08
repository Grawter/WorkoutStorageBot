#region using

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace WorkoutStorageBot.Model.HandlerData.Results.UpdateInfo
{
    public interface IUpdateInfo
    {
        UpdateType UpdateType { get; }

        bool IsExpectedType { get; }

        Update Update { get; }
    }
}