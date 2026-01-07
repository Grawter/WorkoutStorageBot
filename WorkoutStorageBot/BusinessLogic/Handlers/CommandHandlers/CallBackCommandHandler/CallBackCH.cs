#region using
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
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

        internal override IInformationSet GetData()
        {
            if (this.InformationSet == null)
                throw new InvalidOperationException($"Операция '{callbackQueryParser.Direction}|{callbackQueryParser.SubDirection}' вернула пустой {nameof(this.InformationSet)}");

            switch (this.InformationSet)
            {
                case MessageInformationSet MISet:
                    return MISet;

                case FileInformationSet FISet:
                    return FISet;

                default:
                    throw new NotImplementedException($"Неожиданный {nameof(this.InformationSet)}: {this.InformationSet.GetType().Name}");
            }
        }
    }
}