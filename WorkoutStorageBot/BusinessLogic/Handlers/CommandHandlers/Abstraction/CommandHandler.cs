#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.Interfaces;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction
{
    internal abstract class CommandHandler
    {
        protected IEnumerable<HandlerAction> HandlerActions { get; set; } 

        protected IDomain? Domain { get; set; }

        protected IInformationSet InformationSet { get; set; }

        protected CommandHandlerData CommandHandlerTools { get; }

        internal CommandHandler(CommandHandlerData commandHandlerTools)
        {
            this.CommandHandlerTools = CommonHelper.GetIfNotNull(commandHandlerTools);

            this.HandlerActions = Enumerable.Empty<HandlerAction>();
        }

        internal abstract CommandHandler Expectation(params HandlerAction[] handlerActions);

        internal virtual void ClearHandlerAction()
            => this.HandlerActions = Enumerable.Empty<HandlerAction>();
        
        internal abstract IInformationSet GetData();
    }
}