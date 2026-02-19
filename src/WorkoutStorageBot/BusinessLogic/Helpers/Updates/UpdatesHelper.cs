using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Updates
{
    internal static class UpdatesHelper
    {
        internal static IUpdateInfo GetUpdateInfo(Update update)
        {
            ArgumentNullException.ThrowIfNull(update);

            IUpdateInfo? result = null;

            switch (update.Type)
            {
                case UpdateType.Message:

                    if (update.Message != null && update.Message.From != null)
                    {
                        if (update.Message.Type == MessageType.Text)
                            result = new ShortUpdateInfo(update, update.Message.From, update.Message.Chat.Id, update.Message.Text, update.Type, true, update.Message.Type);
                        else
                            result = new ShortUpdateInfo(update, update.Message.From, update.Message.Chat.Id, string.Empty, update.Type, false, update.Message.Type);
                    }
                    break;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery != null && update.CallbackQuery.From != null && update.CallbackQuery.Message != null)
                        result = new ShortUpdateInfo(update, update.CallbackQuery.From, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Data, update.Type, true);
                    break;

                case UpdateType.EditedMessage:
                    if (update.EditedMessage != null && update.EditedMessage.From != null)
                        result = new ShortUpdateInfo(update, update.EditedMessage.From, update.EditedMessage.Chat.Id, update.EditedMessage.Text, update.Type, false);
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