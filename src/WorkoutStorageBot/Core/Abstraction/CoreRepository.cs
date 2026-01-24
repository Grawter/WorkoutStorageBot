using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.HandlerData;

namespace WorkoutStorageBot.Core.Abstraction
{
    internal class CoreRepository : CoreData
    {
        internal string HandlerName { get; }

        protected CoreRepository(CoreTools coreTools, CoreManager coreManager, string handlerName) : base(coreTools, coreManager)
        {
            HandlerName = handlerName;
        }
    }
}