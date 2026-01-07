#region using
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.DTO.HandlerData;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction
{
    internal abstract class CommandHandler
    {
        protected CommandHandlerData CommandHandlerTools { get; }

        internal CommandHandler(CommandHandlerData commandHandlerTools)
        {
            this.CommandHandlerTools = CommonHelper.GetIfNotNull(commandHandlerTools);
        }

        protected void CheckInformationSet(IInformationSet informationSet)
        {
            if (informationSet == null)
                throw new InvalidOperationException($"Операция '{this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget}' вернула пустой {nameof(informationSet)}");
        }

        internal abstract IInformationSet GetInformationSet();
    }
}