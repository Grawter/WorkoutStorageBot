#region using

using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;

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