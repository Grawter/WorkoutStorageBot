using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.Core.Sender
{
    internal interface IBotResponseSender
    {
        Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext);

        Task SendSimpleMassiveNotification(IEnumerable<string> chatIDs, string message);


        Task SendSimpleMassiveNotification(IEnumerable<long> chatIDs, string message);


        Task SendSimpleNotification(string chatID, string message);

        Task SendSimpleNotification(long chatID, string message);

        Task AnswerCallbackQuery(string callbackQueryID);
    }
}