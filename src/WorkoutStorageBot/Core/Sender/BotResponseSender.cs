using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Buttons.Provider;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.Core.Sender
{
    internal class BotResponseSender : IBotResponseSender
    {
        public BotResponseSender(ITelegramBotClient telegramBotClient)
        {
            botClient = telegramBotClient;
        }

        private readonly ITelegramBotClient botClient;

        public async Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(messageInformationSetting);
            ArgumentNullException.ThrowIfNull(currentUserContext);

            InlineButtonsProvider buttonsProvider = new InlineButtonsProvider(currentUserContext);
            ReplyMarkup buttons = buttonsProvider.GetInlineButtons(messageInformationSetting.ButtonsSets, messageInformationSetting.AdditionalParameters);

            string resultText = GetResultText(messageInformationSetting.Message, buttonsProvider.AllVerticalButtonsDisplayed, buttonsProvider.AllHorizontalButtonsDisplayed);

            switch (messageInformationSetting)
            {
                case MessageInformationSet MISet:
                    await botClient.SendMessage(chatId: chatId,
                                                text: resultText,
                                                parseMode: messageInformationSetting.ParseMode,
                                                replyMarkup: buttons,
                                                cancellationToken: cancellationToken);
                    break;

                case FileInformationSet FISet:
                    using (FISet.Stream)
                    {
                        await botClient.SendDocument(chatId: chatId,
                                                     document: InputFile.FromStream(stream: FISet.Stream, fileName: FISet.FileName),
                                                     caption: resultText,
                                                     parseMode: messageInformationSetting.ParseMode,
                                                     replyMarkup: buttons,
                                                     cancellationToken: cancellationToken);
                    }

                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный messageInformationSetting: {messageInformationSetting.GetType()}");
            }
        }

        public async Task SendSimpleMassiveNotification(IEnumerable<string> chatIDs, string message, CancellationToken cancellationToken)
        {
            foreach (string ownerChatId in chatIDs)
            {
                await SendSimpleNotification(ownerChatId, message, cancellationToken);
            }
        }

        public async Task SendSimpleMassiveNotification(IEnumerable<long> chatIDs, string message, CancellationToken cancellationToken)
        {
            foreach (long ownerChatId in chatIDs)
            {
                await SendSimpleNotification(ownerChatId, message, cancellationToken);
            }
        }

        public async Task SendSimpleNotification(string chatID, string message, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(chatID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(message);

            await botClient.SendMessage(chatID, message, cancellationToken: cancellationToken);
        }

        public async Task SendSimpleNotification(long chatID, string message, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(message);

            await botClient.SendMessage(chatID, message, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Не обязательно. Чтобы не было анимации "зависание кнопки" в ТГ боте
        /// </summary>
        public async Task AnswerCallbackQuery(string callbackQueryID, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(callbackQueryID);

            await botClient.AnswerCallbackQuery(callbackQueryID, cancellationToken: cancellationToken);
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