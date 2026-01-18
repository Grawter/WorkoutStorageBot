using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Model.DTO.HandlerData
{
    internal class CommandHandlerData
    {
        internal required EntityContext Db { get; init; }
        internal required CoreData ParentHandler { get; init; }
        internal required UserContext CurrentUserContext { get; init; }
    }
}