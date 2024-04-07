#region using
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.InformationSetForSend;
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

        internal override IInformationSet GetData()
        {
            foreach (var handlerAction in handlerActions)
            {
                switch (handlerAction)
                {
                    case HandlerAction.None:
                        break;
                    case HandlerAction.Update:
                        db.Update(domain);
                        break;
                    case HandlerAction.Add:
                        db.Add(domain);
                        break;
                    case HandlerAction.Remove:
                        db.Remove(domain);
                        break;
                    case HandlerAction.Save:
                        db.SaveChanges();
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный handlerAction: {handlerAction}");
                }
            }

            switch (informationSet)
            {
                case MessageInformationSet MISet:
                    return MISet;

                case FileInformationSet FISet:
                    return FISet;

                default:
                    throw new NotImplementedException($"Неожиданный {nameof(informationSet)}: {informationSet.GetType()}");
            }
        }
    }
}