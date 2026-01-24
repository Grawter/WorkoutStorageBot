using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Helpers.Buttons;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;

namespace WorkoutStorageBot.Core.Sender
{
    internal class BotResponseSender : IBotResponseSender
    {
        public BotResponseSender(ITelegramBotClient telegramBotClient)
        {
            botClient = telegramBotClient;
        }

        private readonly ITelegramBotClient botClient;

        public async Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext)
        {
            ArgumentNullException.ThrowIfNull(messageInformationSetting);
            ArgumentNullException.ThrowIfNull(currentUserContext);

            InlineButtonsHelper buttonsHelper = new InlineButtonsHelper(currentUserContext);
            ReplyMarkup buttons = buttonsHelper.GetInlineButtons(messageInformationSetting.ButtonsSets, messageInformationSetting.AdditionalParameters);

            string resultText = GetResultText(messageInformationSetting.Message, buttonsHelper.AllVerticalButtonsDisplayed, buttonsHelper.AllHorizontalButtonsDisplayed);

            switch (messageInformationSetting)
            {
                case MessageInformationSet MISet:
                    await botClient.SendMessage(chatId: chatId,
                                                text: resultText,
                                                parseMode: messageInformationSetting.ParseMode,
                                                replyMarkup: buttons);
                    break;

                case FileInformationSet FISet:
                    await botClient.SendDocument(chatId: chatId,
                                                 document: InputFile.FromStream(stream: FISet.Stream, fileName: FISet.FileName),
                                                 caption: resultText,
                                                 parseMode: messageInformationSetting.ParseMode,
                                                 replyMarkup: buttons);

                    FISet.Stream.Dispose();
                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный messageInformationSetting: {messageInformationSetting.GetType()}");
            }
        }

        public async Task SendSimpleMassiveNotification(IEnumerable<string> chatIDs, string message)
        {
            foreach (string ownerChatId in chatIDs)
            {
                await SendSimpleNotification(ownerChatId, message);
            }
        }

        public async Task SendSimpleMassiveNotification(IEnumerable<long> chatIDs, string message)
        {
            foreach (long ownerChatId in chatIDs)
            {
                await SendSimpleNotification(ownerChatId, message);
            }
        }

        public async Task SendSimpleNotification(string chatID, string message)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(chatID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(message);

            await botClient.SendMessage(chatID, message);
        }

        public async Task SendSimpleNotification(long chatID, string message)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(message);

            await botClient.SendMessage(chatID, message);
        }

        /// <summary>
        /// Не обязательно. Чтобы не было анимации "зависание кнопки" в ТГ боте
        /// </summary>
        public async Task AnswerCallbackQuery(string callbackQueryID)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(callbackQueryID);

            await botClient.AnswerCallbackQuery(callbackQueryID);
        }

        private string GetResultText(string originalText, bool allVerticalButtonsDisplayed, bool allHorizontalButtonsDisplayed)
        {
            if (!allVerticalButtonsDisplayed)
            {
                if (!allHorizontalButtonsDisplayed)
                    return $"{originalText}{Environment.NewLine}(Были отображены первые {CommonConsts.Buttons.MaxVerticalButtonsCount} кнопок по вертикали и {CommonConsts.Buttons.MaxHorizontalButtonsCount} по горизонтали)";
                else
                    return $"{originalText}{Environment.NewLine}(Были отображены первые {CommonConsts.Buttons.MaxVerticalButtonsCount} кнопок по вертикали)";
            }

            if (!allHorizontalButtonsDisplayed)
                return $"{originalText}{Environment.NewLine}(Были отображены первые {CommonConsts.Buttons.MaxHorizontalButtonsCount} кнопки по горизонтали)";

            return originalText;
        }
    }
}