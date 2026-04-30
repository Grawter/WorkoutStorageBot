using WorkoutStorageBot.Model.DTO.HandlerData;

namespace WorkoutStorageBot.Core.Handlers.Abstraction
{
    internal abstract class CoreData
    {
        internal CoreTools CoreTools { get; }

        protected CoreData(CoreTools coreTools)
        {
            this.CoreTools = coreTools;
        }
    }
}