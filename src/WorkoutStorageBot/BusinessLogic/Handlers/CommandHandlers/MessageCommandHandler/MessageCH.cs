using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Model.DTO.HandlerData;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.MessageCommandHandler
{
    internal abstract class MessageCH : CommandHandler
    {
        protected readonly MessageTextBuilder requestTextBuilder;
        internal MessageCH(CommandHandlerTools commandHandlerTools, MessageTextBuilder requestTextBuilder) : base(commandHandlerTools)
        {
            this.requestTextBuilder = requestTextBuilder;
        }

        protected void CheckInformationSet(IInformationSet informationSet)
        {
            if (informationSet == null)
                throw new InvalidOperationException($"Операция '{this.CurrentUserContext.Navigation.MessageNavigationTarget}' вернула пустой {nameof(informationSet)}");
        }
    }
}