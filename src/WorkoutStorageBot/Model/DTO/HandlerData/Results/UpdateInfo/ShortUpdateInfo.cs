using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo
{
    internal class ShortUpdateInfo : IUpdateInfo
    {
        internal ShortUpdateInfo(Update update, User user, long chatId, string? data, UpdateType updateType, bool isExpectedType, MessageType messageType = MessageType.Unknown)
        {
            Update = update.ThrowIfNull();
            User = user.ThrowIfNull();
            ChatId = chatId > 0 ? chatId : throw new ArgumentNullException(nameof(chatId));
            UpdateType = updateType;
            IsExpectedType = isExpectedType;
            MessageType = messageType;

            if (!string.IsNullOrWhiteSpace(data))
                Data = data;
            else
            {
                Data = $"Unknown or empty data";

                IsExpectedType = false;
            }
        }

        internal User User { get; }

        internal long ChatId { get; }

        internal string Data { get; }

        internal MessageType MessageType { get; }

        public UpdateType UpdateType { get; }

        public bool IsExpectedType { get; }

        public Update Update { get; }
    }
}