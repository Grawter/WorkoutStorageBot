using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.Core.Sender
{
    public interface IBotResponseSender
    {
        Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext, CancellationToken cancellationToken);

        Task SendSimpleMassiveNotification(IEnumerable<string> chatIDs, string message, CancellationToken cancellationToken);


        Task SendSimpleMassiveNotification(IEnumerable<long> chatIDs, string message, CancellationToken cancellationToken);


        Task SendSimpleNotification(string chatID, string message, CancellationToken cancellationToken);

        Task SendSimpleNotification(long chatID, string message, CancellationToken cancellationToken);

        Task AnswerCallbackQuery(string callbackQueryID, CancellationToken cancellationToken);
    }
}