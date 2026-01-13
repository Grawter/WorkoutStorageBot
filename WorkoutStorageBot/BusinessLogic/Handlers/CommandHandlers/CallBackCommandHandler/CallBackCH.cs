using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Model.DTO.HandlerData;

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

        protected void CheckInformationSet(IInformationSet informationSet)
        {
            if (informationSet == null)
                throw new InvalidOperationException($"Операция '{callbackQueryParser.Direction}|{callbackQueryParser.SubDirection}' вернула пустой {nameof(informationSet)}");
        }
    }
}