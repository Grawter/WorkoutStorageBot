#region using

using Telegram.Bot.Types;
using Telegram.Bot;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.Buttons;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Sender
{
    internal class BotResponseSender : IBotResponseSender
    {
        public BotResponseSender(ITelegramBotClient telegramBotClient)
        {
            botClient = CommonHelper.GetIfNotNull(telegramBotClient);
        }

        private readonly ITelegramBotClient botClient;

        public async Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext)
        {
            ArgumentNullException.ThrowIfNull(messageInformationSetting);
            ArgumentNullException.ThrowIfNull(currentUserContext);

            InlineButtonsHelper buttons = new InlineButtonsHelper(currentUserContext);

            // Не обязательно. Чтобы не было анимации "зависание кнопки" в ТГ боте
            if (messageInformationSetting.AdditionalParameters.TryGetValue("BotCallBackID", out string callbackQueryID))
                await AnswerCallbackQuery(callbackQueryID);

            switch (messageInformationSetting)
            {
                case MessageInformationSet MISet:
                    await botClient.SendMessage(chatId,
                                            messageInformationSetting.Message,
                                            ParseMode.Html,
                                            replyMarkup: buttons.GetInlineButtons(messageInformationSetting.ButtonsSets, messageInformationSetting.AdditionalParameters));
                    break;

                case FileInformationSet FISet:
                    await botClient.SendDocument(chatId,
                                            document: InputFile.FromStream(stream: FISet.Stream, fileName: FISet.FileName),
                                            caption: FISet.Message,
                                            ParseMode.Html,
                                            replyMarkup: buttons.GetInlineButtons(messageInformationSetting.ButtonsSets, messageInformationSetting.AdditionalParameters));

                    FISet.Stream.Dispose();
                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный messageInformationSetting: {messageInformationSetting.GetType()}");
            }
        }

        public async Task SendSimpleMassiveResponse(string[] chatIDs, string message)
        {
            foreach (string ownerChatId in chatIDs)
            {
                await SimpleNotification(ownerChatId, message);
            }
        }

        public async Task SendSimpleMassiveResponse(long[] chatIDs, string message)
        {
            foreach (long ownerChatId in chatIDs)
            {
                await SimpleNotification(ownerChatId, message);
            }
        }

        public async Task SimpleNotification(string chatID, string message)
           => await botClient.SendMessage(chatID, message);

        public async Task SimpleNotification(long chatID, string message)
            => await botClient.SendMessage(chatID, message);

        /// <summary>
        /// Не обязательно. Чтобы не было анимации "зависание кнопки" в ТГ боте
        /// </summary>
        private async Task AnswerCallbackQuery(string callbackQueryID)
            =>  await botClient.AnswerCallbackQuery(callbackQueryID);
    }
}