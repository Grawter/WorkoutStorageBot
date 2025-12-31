#region using
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction;
using WorkoutStorageBot.Model.DTO.HandlerData;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler
{
    internal abstract class MessageCH : CommandHandler
    {
        protected readonly TextMessageConverter requestConverter;
        internal MessageCH(CommandHandlerData commandHandlerTools, TextMessageConverter requestConverter) : base(commandHandlerTools)
        {
            this.requestConverter = requestConverter;
        }

        internal override IInformationSet GetData()
        {
            foreach (HandlerAction handlerAction in this.HandlerActions)
            {
                if (this.Domain == null && handlerAction != HandlerAction.None && handlerAction != HandlerAction.Save)
                    throw new NullReferenceException($"{nameof(this.Domain)} is null!");

                switch (handlerAction)
                {
                    case HandlerAction.None:
                        break;
                    case HandlerAction.Update:
                        CommandHandlerTools.Db.Update(this.Domain);
                        break;
                    case HandlerAction.Add:
                        CommandHandlerTools.Db.Add(this.Domain);
                        break;
                    case HandlerAction.Remove:
                        CommandHandlerTools.Db.Remove(this.Domain);
                        break;
                    case HandlerAction.Save:
                        CommandHandlerTools.Db.SaveChanges();
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный {nameof(handlerAction)}: {handlerAction}");
                }
            }

            if (this.InformationSet == null)
                throw new InvalidOperationException($"Операция '{this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget}' вернула пустой {nameof(this.InformationSet)}");

            switch (this.InformationSet)
            {
                case MessageInformationSet MISet:
                    return MISet;

                case FileInformationSet FISet:
                    return FISet;

                default:
                    throw new NotImplementedException($"Неожиданный {nameof(this.InformationSet)}: {this.InformationSet.GetType()}");
            }
        }
    }
}