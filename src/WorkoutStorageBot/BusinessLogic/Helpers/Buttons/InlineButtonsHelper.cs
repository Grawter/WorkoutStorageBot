using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Buttons.Storage;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Buttons
{
    internal class InlineButtonsHelper
    {
        private UserContext CurrentUserContext { get; }

        internal bool AllVerticalButtonsDisplayed { get; private set; }
        internal bool AllHorizontalButtonsDisplayed { get; private set; }

        internal InlineButtonsHelper(UserContext userContext)
        {
            CurrentUserContext = userContext;
        }

        internal ReplyMarkup GetInlineButtons(ButtonsSet buttonsSet, Dictionary<string, string>? additionalParameters = null)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSet, ButtonsSet.None, additionalParameters));
        }

        internal ReplyMarkup GetInlineButtons((ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets, Dictionary<string, string>? additionalParameters = null)
        {
            return new InlineKeyboardMarkup(GetButtons(buttonsSets.buttonsSet, buttonsSets.backButtonsSet, additionalParameters));
        }

        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtons(ButtonsSet buttonsSet, ButtonsSet backButtonsSet, Dictionary<string, string>? additionalParameters)
        {
            ButtonsFactory buttonsFactory = ButtonsStorage.GetButtonsFactory(buttonsSet, CurrentUserContext);

            IEnumerable<IEnumerable<InlineKeyboardButton>> buttons = buttonsFactory.CreateButtons(backButtonsSet, additionalParameters);

            AllVerticalButtonsDisplayed = buttonsFactory.AllVerticalButtonsDisplayed;
            AllHorizontalButtonsDisplayed = buttonsFactory.AllHorizontalButtonsDisplayed;

            return buttons;
        }
    }
}