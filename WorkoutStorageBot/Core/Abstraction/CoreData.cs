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

        protected CoreData(CoreTools coreTools)
        {
            this.CoreTools = CommonHelper.GetIfNotNull(coreTools);

            ArgumentNullException.ThrowIfNull(coreTools.Db);
        }

        protected CoreData(CoreTools coreTools, CoreManager coreManager) : this(coreTools)
        {
            this.CoreManager = CommonHelper.GetIfNotNull(coreManager);
        }

        internal bool TryResetCoreManager(CoreManager coreManager)
        {
            if (this.CoreManager != null)
                return false;

            this.CoreManager = coreManager;

            return true;
        }
    }
}