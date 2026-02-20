using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler
{
    internal abstract class CallBackCH : CommandHandler
    {
        protected readonly CallbackQueryParser callbackQueryParser;
        internal CallBackCH(CommandHandlerTools commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools)
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