#region using
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction;
using WorkoutStorageBot.Model.DTO.HandlerData;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler
{
    internal abstract class CallBackCH : CommandHandler
    {
        protected readonly CallbackQueryParser callbackQueryParser;
        internal CallBackCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) 
            : base(commandHandlerTools)
        {
            this.callbackQueryParser = callbackQueryParser;
        }
    }
}