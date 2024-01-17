#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.InformationSetForSend;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler
{
    internal abstract class CommandHandler
    {
        protected readonly EntityContext db;
        protected readonly UserContext currentUserContext;
        protected DomainProvider? domainProvider;
        protected IEnumerable<HandlerAction> handlerActions;

        protected IDomain? domain;
        protected ResponseConverter responseConverter;
        protected (ButtonsSet, ButtonsSet) buttonsSets;

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