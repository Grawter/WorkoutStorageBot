using WorkoutStorageBot.Model.AppContext;

namespace WorkoutStorageBot.Core.Repositories.Abstraction
{
    internal class CoreRepository 
    {
        internal readonly string handlerName;
        protected readonly EntityContext db;

        protected CoreRepository(EntityContext db, string handlerName) 
        {
            this.db = db;
            this.handlerName = handlerName;
        }
    }
}