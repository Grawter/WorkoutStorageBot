#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.InformationSetForSend;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler
{
    internal abstract class CommandHandler
    {
        protected readonly EntityContext db;
        protected readonly UserContext currentUserContext;
        protected IEnumerable<HandlerAction> handlerActions;

        protected IDomain? domain;

        protected IInformationSet informationSet;

        internal CommandHandler(EntityContext db, UserContext userContext)
        {
            this.db = db;
            currentUserContext = userContext;
            handlerActions = Enumerable.Empty<HandlerAction>();
        }

        internal abstract CommandHandler Expectation(params HandlerAction[] handlerActions);

        internal virtual void ClearHandlerAction()
        {
            handlerActions = Enumerable.Empty<HandlerAction>();
        }

        internal abstract IInformationSet GetData();
    }
}