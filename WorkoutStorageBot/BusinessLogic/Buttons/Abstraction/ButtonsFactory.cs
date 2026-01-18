using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Helpers.Crypto;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Abstraction
{
    internal abstract class ButtonsFactory
    {
        private List<List<InlineKeyboardButton>> inlineKeyboardButtonsMain;
        private List<InlineKeyboardButton>? inlineKeyboardButtons;
        
        private int horizontalButtonsCounter = 0;
        private int verticalButtonsCounter = 0;

        internal bool AllVerticalButtonsDisplayed { get; private set; } = true;
        internal bool AllHorizontalButtonsDisplayed { get; private set; } = true;

        protected UserContext CurrentUserContext { get; }

        internal ButtonsFactory(UserContext userContext)
        {
            CurrentUserContext = userContext;

            CurrentUserContext.CallBackId = CryptographyHelper.CreateRandomCallBackQueryId();

            inlineKeyboardButtonsMain = new();
        }

        internal IEnumerable<IEnumerable<InlineKeyboardButton>> CreateButtons(ButtonsSet backButtonsSet = ButtonsSet.None, Dictionary<string, string>? additionalParameters = null)
        {
            AddBusinessButtons(additionalParameters);

            if (backButtonsSet != ButtonsSet.None)
                AddInlineButton("Назад", $"0|Back||{backButtonsSet}");

            if (inlineKeyboardButtons.HasItemsInCollection())
                inlineKeyboardButtonsMain.Add(inlineKeyboardButtons);

            return inlineKeyboardButtonsMain;
        }

        internal abstract void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null);

        protected void AddInlineButton(string titleButton, string callBackDataWithoutId, bool onNewLine = true)
        {
            string callBackData = AddCallBackId(callBackDataWithoutId);

            if (onNewLine)
            {
                ++verticalButtonsCounter;

                if (verticalButtonsCounter > CommonConsts.Buttons.MaxVerticalButtonsCount)
                {
                    AllVerticalButtonsDisplayed = false;
                    return;
                }

                List<InlineKeyboardButton> oneLineKeyboardButton = new() { InlineKeyboardButton.WithCallbackData(titleButton, callBackData) };

                inlineKeyboardButtonsMain.Add(oneLineKeyboardButton);
            }
            else
            {
                ++horizontalButtonsCounter;

                if (horizontalButtonsCounter > CommonConsts.Buttons.MaxHorizontalButtonsCount)
                {
                    AllHorizontalButtonsDisplayed = false;
                    return;
                }

                if (inlineKeyboardButtons is null)
                    inlineKeyboardButtons = new();

                inlineKeyboardButtons.Add(InlineKeyboardButton.WithCallbackData(titleButton, callBackData));
            }
        }

        private string AddCallBackId(string callBackDataWithoutId)
        {
            callBackDataWithoutId += $"|{CurrentUserContext.CallBackId}";

            return callBackDataWithoutId;
        }

        protected void GetDomainsInButtons(IEnumerable<IDTODomain>? source, string subDirection, bool isNeedShowID = false)
        {
            if (source.HasItemsInCollection())
            {
                foreach (IDTODomain domain in source)
                {
                    if (isNeedShowID)
                        AddInlineButton($"{domain.Name}[Id:{domain.Id}]", $"2|{subDirection}|{domain.GetType().Name.Replace("DTO", "")}|{domain.Id}");
                    else
                        AddInlineButton(domain.Name, $"2|{subDirection}|{domain.GetType().Name.Replace("DTO", "")}|{domain.Id}");
                }
            }
        }
    }
}