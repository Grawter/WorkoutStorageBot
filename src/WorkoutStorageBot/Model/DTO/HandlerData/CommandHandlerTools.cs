using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Abstraction;

namespace WorkoutStorageBot.Model.DTO.HandlerData
{
    internal class CommandHandlerTools
    {
        internal required CoreHandler ParentHandler { get; init; }
        internal required UserContext CurrentUserContext { get; init; }
    }
}