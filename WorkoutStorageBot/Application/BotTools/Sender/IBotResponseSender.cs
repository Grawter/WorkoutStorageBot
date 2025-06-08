#region using

using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.Application.BotTools.Sender
{
    internal interface IBotResponseSender
    {
        Task SendResponse(long chatId, IInformationSet messageInformationSetting, UserContext currentUserContext);

        Task SendSimpleMassiveResponse(string[] chatIDs, string message);


        Task SendSimpleMassiveResponse(long[] chatIDs, string message);


        Task SimpleNotification(string chatID, string message);

        Task SimpleNotification(long chatID, string message);
    }
}