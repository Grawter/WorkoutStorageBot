using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.DTO.HandlerData;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction
{
    internal abstract class CommandHandler
    {
        protected CommandHandlerData CommandHandlerTools { get; }

        internal CommandHandler(CommandHandlerData commandHandlerTools)
        {
            this.CommandHandlerTools = CommonHelper.GetIfNotNull(commandHandlerTools);
        }

        internal abstract IInformationSet GetInformationSet();
    }
}