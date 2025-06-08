#region using

using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.HandlerData;

#endregion

namespace WorkoutStorageBot.Core.Abstraction
{
    internal class CoreRepository : CoreData
    {
        internal string HandlerName { get; }

        protected CoreRepository(CoreTools coreTools, CoreManager coreManager, string handlerName) : base(coreTools, coreManager)
        {
            HandlerName = handlerName;
        }

        protected CoreRepository(CoreTools coreTools, string handlerName) : base(coreTools)
        { 
            HandlerName = handlerName;
        }
    }
}