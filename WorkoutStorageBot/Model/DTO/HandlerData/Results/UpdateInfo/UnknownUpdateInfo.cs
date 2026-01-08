using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Helpers.Common;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo
{
    internal class UnknownUpdateInfo : IUpdateInfo
    {
        internal UnknownUpdateInfo(Update update, UpdateType updateType)
        {
            Update = CommonHelper.GetIfNotNull(update);
            UpdateType = updateType;
            IsExpectedType = false;
        }

        public UpdateType UpdateType { get; }

        public bool IsExpectedType { get; }

        public Update Update { get; }
    }
}