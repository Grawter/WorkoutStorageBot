using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.DTO.HandlerData;

namespace WorkoutStorageBot.Core.Abstraction
{
    internal abstract class CoreData
    {
        internal CoreTools CoreTools { get; }

        internal CoreManager CoreManager { get; }

        protected CoreData(CoreTools coreTools, CoreManager coreManager)
        {
            this.CoreManager = CommonHelper.GetIfNotNull(coreManager);
            this.CoreTools = CommonHelper.GetIfNotNull(coreTools);
        }
    }
}