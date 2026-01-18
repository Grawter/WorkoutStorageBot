using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Model.DTO.HandlerData;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction
{
    internal abstract class CommandHandler
    {
        protected CommandHandlerData CommandHandlerTools { get; }

        internal CommandHandler(CommandHandlerData commandHandlerTools)
        {
            this.CommandHandlerTools = commandHandlerTools;
        }

        internal abstract Task<IInformationSet> GetInformationSet();
    }
}