#region using
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Model;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using Telegram.Bot.Types;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler
{
    internal abstract class CallBackCH : CommandHandler
    {
        protected readonly CallbackQueryParser callbackQueryParser;
        internal CallBackCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) 
            : base(db, userContext)
        {
            this.callbackQueryParser = callbackQueryParser;
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