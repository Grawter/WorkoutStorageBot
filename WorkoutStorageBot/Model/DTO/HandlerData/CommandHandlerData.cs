using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Model.DTO.HandlerData
{
    internal class CommandHandlerData
    {
        internal EntityContext Db { get; init; }
        internal CoreData ParentHandler { get; init; }
        internal UserContext CurrentUserContext { get; init; }
    }
}