using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

namespace WorkoutStorageBot.Helpers.Updates
{
    internal static class UpdatesHelper
    {
        internal static IUpdateInfo GetUpdateInfo(Update update)
        {
            ArgumentNullException.ThrowIfNull(update);

            IUpdateInfo result;

            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message.Type == MessageType.Text)
                        result = new ShortUpdateInfo(update, update.Message.From, update.Message.Chat.Id, update.Message.Text, update.Type, true);
                    else
                        result = new ShortUpdateInfo(update, update.Message.From, update.Message.Chat.Id, null, update.Type, false);
                    break;
                case UpdateType.CallbackQuery:
                    result = new ShortUpdateInfo(update, update.CallbackQuery.From, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Data, update.Type, true);
                    break;

                case UpdateType.EditedMessage:
                    result = new ShortUpdateInfo(update, update.EditedMessage.From, update.EditedMessage.Chat.Id, null, update.Type, false);
                    break;

                case UpdateType.ChannelPost:
                    result = new ShortUpdateInfo(update, update.ChannelPost.From, update.ChannelPost.Chat.Id, null, update.Type, false);
                    break;

                case UpdateType.EditedChannelPost:
                    result = new ShortUpdateInfo(update, update.EditedChannelPost.From, update.EditedChannelPost.Chat.Id, null, update.Type, false);
                    break;

                case UpdateType.MyChatMember:
                    result = new ShortUpdateInfo(update, update.MyChatMember.From, update.MyChatMember.Chat.Id, null, update.Type, false);
                    break;

                case UpdateType.ChatMember:
                    result = new ShortUpdateInfo(update, update.ChatMember.From, update.ChatMember.Chat.Id, null, update.Type, false);
                    break;

                case UpdateType.ChatJoinRequest:
                    result = new ShortUpdateInfo(update, update.ChatJoinRequest.From, update.ChatJoinRequest.Chat.Id, null, update.Type, false);
                    break;

                default:
                    result = new UnknownUpdateInfo(update, update.Type);
                    break;
            }

            return result;
        }
    }
}
