using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Updates
{
    internal static class UpdatesHelper
    {
        private const string NotSupportedData = "NotSupportedData";

        internal static IUpdateInfo GetUpdateInfo(Update update)
        {
            ArgumentNullException.ThrowIfNull(update);

            IUpdateInfo? result = null;

            switch (update.Type)
            {
                case UpdateType.Message:

                    if (update.Message != null)
                    {
                        if (update.Message.Type == MessageType.Text)
                            result = new ShortUpdateInfo(update, update.Message.From, update.Message.Chat.Id, update.Message.Text, update.Type, true);
                        else
                            result = new ShortUpdateInfo(update, update.Message.From, update.Message.Chat.Id, update.Message.Type.ToString(), update.Type, false);
                    }
                    break;
                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery != null && update.CallbackQuery.Message != null)
                        result = new ShortUpdateInfo(update, update.CallbackQuery.From, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Data, update.Type, true);
                    break;

                case UpdateType.EditedMessage:
                    if (update.EditedMessage != null)
                        result = new ShortUpdateInfo(update, update.EditedMessage.From, update.EditedMessage.Chat.Id, NotSupportedData, update.Type, false);
                    break;

                case UpdateType.ChannelPost:
                    if (update.ChannelPost != null)
                        result = new ShortUpdateInfo(update, update.ChannelPost.From, update.ChannelPost.Chat.Id, NotSupportedData, update.Type, false);
                    break;

                case UpdateType.EditedChannelPost:
                    if (update.EditedChannelPost != null)
                        result = new ShortUpdateInfo(update, update.EditedChannelPost.From, update.EditedChannelPost.Chat.Id, NotSupportedData, update.Type, false);
                    break;

                case UpdateType.MyChatMember:
                    if (update.MyChatMember != null)
                        result = new ShortUpdateInfo(update, update.MyChatMember.From, update.MyChatMember.Chat.Id, NotSupportedData, update.Type, false);
                    break;

                case UpdateType.ChatMember:
                    if (update.ChatMember != null)
                        result = new ShortUpdateInfo(update, update.ChatMember.From, update.ChatMember.Chat.Id, NotSupportedData, update.Type, false);
                    break;

                case UpdateType.ChatJoinRequest:
                    if (update.ChatJoinRequest != null)
                        result = new ShortUpdateInfo(update, update.ChatJoinRequest.From, update.ChatJoinRequest.Chat.Id, NotSupportedData, update.Type, false);
                    break;

                default:
                    result = new UnknownUpdateInfo(update, update.Type);
                    break;
            }

            if (result == null)
                result = new UnknownUpdateInfo(update, update.Type);

            return result;
        }
    }
}