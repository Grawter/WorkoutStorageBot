using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.HandlerData.Results.UpdateInfo;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.Model.DTO.HandlerData.Results
{
    internal class AuthorizedHandlerResult : HandlerResult
    {
        internal required ShortUpdateInfo ShortUpdateInfo { get; init; }

        internal required UserContext CurrentUserContext { get; init; }

        internal IInformationSet? InformationSet { get; set; }
    }
}