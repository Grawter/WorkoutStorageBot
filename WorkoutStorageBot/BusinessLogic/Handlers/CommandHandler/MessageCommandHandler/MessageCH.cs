#region using
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.Converters;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.MessageCommandHandler
{
    internal abstract class MessageCH : CommandHandler
    {
        protected readonly TextMessageConverter requestConverter;
        internal MessageCH(EntityContext db, UserContext userContext, TextMessageConverter requestConverter) : base(db, userContext)
        {
            this.requestConverter = requestConverter;
        }
    }
}