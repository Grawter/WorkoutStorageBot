#region

using Telegram.Bot.Types;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;

#endregion

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