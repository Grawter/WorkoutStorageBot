#region using

using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.HandlerData;

#endregion

namespace WorkoutStorageBot.Core.Abstraction
{
    internal abstract class CoreData
    {
        internal CoreTools CoreTools { get; }

        internal CoreManager CoreManager { get; private set; }

        protected CoreData(CoreTools coreTools, CoreManager coreManager)
        {
            this.CoreManager = CommonHelper.GetIfNotNull(coreManager);
            this.CoreTools = CommonHelper.GetIfNotNull(coreTools);
        }
    }
}