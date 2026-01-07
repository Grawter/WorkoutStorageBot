#region using

using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.Crypto;
using WorkoutStorageBot.Model.Interfaces;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.Abstraction
{
    internal abstract class ButtonsFactory
    {
        private List<List<InlineKeyboardButton>> inlineKeyboardButtonsMain;
        private List<InlineKeyboardButton> inlineKeyboardButtons;

        protected UserContext CurrentUserContext { get; }

        internal ButtonsFactory(UserContext userContext)
        {
            CurrentUserContext = CommonHelper.GetIfNotNull(userContext);

            CurrentUserContext.CallBackId = CryptographyHelper.CreateRandomCallBackQueryId();

            inlineKeyboardButtonsMain = new();
        }

        internal IEnumerable<IEnumerable<InlineKeyboardButton>> Create(ButtonsSet backButtonsSet = ButtonsSet.None, Dictionary<string, string>? additionalParameters = null)
        {
            AddBusinessButtons(additionalParameters);

            if (backButtonsSet != ButtonsSet.None)
                AddInlineButton("Назад", $"0|Back||{backButtonsSet}");

            return inlineKeyboardButtonsMain;
        }

        internal abstract void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null);

        protected void AddInlineButton(string titleButton, string callBackDataWithoutId, bool onNewLine = true)
        {
            string callBackData = AddCallBackId(callBackDataWithoutId);

            if (onNewLine)
            {
                inlineKeyboardButtons = new() { InlineKeyboardButton.WithCallbackData(titleButton, callBackData) };

                inlineKeyboardButtonsMain.Add(inlineKeyboardButtons);
            }
            else
            {
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

        protected void GetDomainsInButtons(IEnumerable<IDTODomain> source, string subDirection)
        {
            if (source.HasItemsInCollection())
            {
                foreach (IDTODomain domain in source)
                {
                    AddInlineButton(domain.Name, $"2|{subDirection}|{domain.GetType().Name.Replace("DTO", "")}|{domain.Id}");
                }
            }
        }
    }
}