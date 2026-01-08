using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Helpers.Common;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo
{
    internal class ShortUpdateInfo : IUpdateInfo
    {
        internal ShortUpdateInfo(Update update, User user, long chatId, string data, UpdateType updateType, bool isExpectedType)
        {
            Update = CommonHelper.GetIfNotNull(update);
            User = user;
            ChatId = chatId;
            Data = data;
            UpdateType = updateType;
            IsExpectedType = isExpectedType;
        }

        internal User User { get; }

        internal long ChatId { get; }

        internal string Data { get; }

        public UpdateType UpdateType { get; }

        public bool IsExpectedType { get; }

        public Update Update { get; }
    }
}