using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Abstraction
{
    internal abstract class ButtonsFactory
    {
        private List<List<InlineKeyboardButton>> inlineKeyboardButtonsMain;
        private List<InlineKeyboardButton>? inlineKeyboardButtons;

        internal int HorizontalButtonsCountAtCurrentLine => inlineKeyboardButtons?.Count ?? 0;
        internal int VerticalButtonsCount => inlineKeyboardButtonsMain.Count;

        internal bool AllVerticalButtonsDisplayed { get; private set; } = true;
        internal bool AllHorizontalButtonsDisplayed { get; private set; } = true;

        protected UserContext CurrentUserContext { get; }

        internal ButtonsFactory(UserContext userContext)
        {
            CurrentUserContext = userContext;

            CurrentUserContext.GenerateNewCallBackId();

            inlineKeyboardButtonsMain = new(CommonConsts.Buttons.MaxVerticalButtonsCount);
        }

        internal IEnumerable<IEnumerable<InlineKeyboardButton>> CreateButtons(ButtonsSet backButtonsSet = ButtonsSet.None, Dictionary<string, string>? additionalParameters = null)
        {
            AddBusinessButtons(additionalParameters);

            if (backButtonsSet != ButtonsSet.None)
                AddBackButton(backButtonsSet);

            return inlineKeyboardButtonsMain;
        }

        protected abstract void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null);

        protected void AddInlineButton(string titleButton, string callBackDataWithoutId, bool onNewLine = true)
        {
            string callBackData = AddCallBackId(callBackDataWithoutId);

            if (onNewLine)
            {
                if (inlineKeyboardButtonsMain.Count >= CommonConsts.Buttons.MaxVerticalButtonsCount)
                {
                    AllVerticalButtonsDisplayed = false;
                    return;
                }

                List<InlineKeyboardButton> oneLineKeyboardButton = new() { InlineKeyboardButton.WithCallbackData(titleButton, callBackData) };

                inlineKeyboardButtonsMain.Add(oneLineKeyboardButton);
            }
            else
            {
                if (inlineKeyboardButtons?.Count >= CommonConsts.Buttons.MaxHorizontalButtonsCount)
                {
                    AllHorizontalButtonsDisplayed = false;
                    return;
                }

                if (inlineKeyboardButtons is null)
                    inlineKeyboardButtons = new(CommonConsts.Buttons.MaxHorizontalButtonsCount);

                inlineKeyboardButtons.Add(InlineKeyboardButton.WithCallbackData(titleButton, callBackData));
            }
        }

        protected void ReplaceInlineButton(string titleButton, string callBackDataWithoutId, bool onNewLine, int replacedButtonIndex)
        {
            string callBackData = AddCallBackId(callBackDataWithoutId);

            if (onNewLine)
            {
                List<InlineKeyboardButton> lastReplacedButton = new() { InlineKeyboardButton.WithCallbackData(titleButton, callBackData) };
                inlineKeyboardButtonsMain[replacedButtonIndex] = lastReplacedButton;
            }
            else
            {
                if (inlineKeyboardButtons is null)
                    inlineKeyboardButtons = new(CommonConsts.Buttons.MaxHorizontalButtonsCount);

                inlineKeyboardButtons[replacedButtonIndex] = InlineKeyboardButton.WithCallbackData(titleButton, callBackData);
            }
        }

        protected void SaveHorizontalButtons()
        {
            if (inlineKeyboardButtons.HasItemsInCollection())
            {
                inlineKeyboardButtonsMain.Add(inlineKeyboardButtons.ToList());

                inlineKeyboardButtons.Clear();
            }
        }

        protected void GetDomainsInButtons(IEnumerable<IDTODomain>? source, string subDirection, bool isNeedShowID = false)
        {
            if (source.HasItemsInCollection())
            {
                foreach (IDTODomain domain in source)
                {
                    if (isNeedShowID)
                        AddInlineButton($"{domain.Name}[Id:{domain.Id}]", $"2|{subDirection}|{domain.GetType().Name.Replace("DTO", string.Empty)}|{domain.Id}");
                    else
                        AddInlineButton(domain.Name, $"2|{subDirection}|{domain.GetType().Name.Replace("DTO", string.Empty)}|{domain.Id}");
                }
            }
        }

        private void AddBackButton(ButtonsSet backButtonsSet)
        {
            if (inlineKeyboardButtonsMain.Count < CommonConsts.Buttons.MaxVerticalButtonsCount)
                AddInlineButton("Назад", $"0|Back||{backButtonsSet}");
            else
                ReplaceInlineButton("Назад", $"0|Back||{backButtonsSet}", true, inlineKeyboardButtonsMain.Count - 1);
        }

        private string AddCallBackId(string callBackDataWithoutId)
        {
            callBackDataWithoutId += $"|{CurrentUserContext.CallBackId}";

            return callBackDataWithoutId;
        }
    }
}