#region using
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler
{
    internal abstract class CallBackCH : CommandHandler
    {
        protected readonly CallbackQueryParser callbackQueryParser;
        internal CallBackCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) : base(db, userContext)
        {
            this.callbackQueryParser = callbackQueryParser;
        }
    }
}