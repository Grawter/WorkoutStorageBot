using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo
{
    internal class ShortUpdateInfo : IUpdateInfo
    {
        internal ShortUpdateInfo(Update update, User? user, long? chatId, string? data, UpdateType updateType, bool isExpectedType)
        {
            Update = update;
            User = user.ThrowIfNull();
            ChatId = chatId.ThrowIfNull();
            Data = data.ThrowIfNullOrWhiteSpace();
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