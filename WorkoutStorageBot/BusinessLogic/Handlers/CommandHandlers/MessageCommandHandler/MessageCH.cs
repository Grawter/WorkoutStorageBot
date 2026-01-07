#region using
using WorkoutStorageBot.Helpers.Converters;
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
    }
}