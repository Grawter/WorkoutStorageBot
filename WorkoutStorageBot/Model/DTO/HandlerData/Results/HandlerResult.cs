using Telegram.Bot.Types;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results
{
    internal class HandlerResult
    {
        internal Update Update { get; init; }

        internal ShortUpdateInfo ShortUpdateInfo { get; set; }

        internal UserContext CurrentUserContext { get; set; }

        internal IInformationSet? InformationSet { get; set; }

        internal bool HasAccess { get; set; }

        internal bool IsNeedContinue { get; set; }
    }
}