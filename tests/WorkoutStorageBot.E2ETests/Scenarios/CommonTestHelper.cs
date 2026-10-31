using FluentAssertions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WorkoutStorageBot.BusinessLogic.Buttons.Provider;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Handlers.Abstraction;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

namespace WorkoutStorageBot.E2ETests.Scenarios
{
    internal class CommonTestsHelper
    {
        private readonly IBotResponseSender botResponseSender;
        private readonly List<CoreHandler> handlers;
        private readonly string userName;
        private readonly long userID;
        private readonly string firstName;

        internal CommonTestsHelper(IBotResponseSender botResponseSender,
                                   List<CoreHandler> handlers,
                                   string userName, 
                                   long userID, 
                                   string firstName,

                                   string firstCycleName = "1",
                                   string secondCycleName = "2",

                                   string firstDayNameInFirstCycle = "11",
                                   string secondDayNameInFirstCycle = "12",
                                   string firstDayNameInSecondCycle = "21",
                                   string secondDayNameInSecondCycle = "22",

                                   string firstExerciseNameOnFirstDayInFirstCycle = "111",
                                   string secondExerciseNameOnFirstDayInFirstCycle = "112",
                                   string firstExerciseNameOnSecondDayInFirstCycle = "121",
                                   string secondExerciseNameOnSecondDayInFirstCycle = "122",
                                   string firstExerciseNameOnFirstDayInSecondCycle = "211",
                                   string secondExerciseNameOnFirstDayInSecondCycle = "212",
                                   string firstExerciseNameOnSecondDayInSecondycle = "221",
                                   string secondExerciseNameOnSecondDayInSecondCycle = "222")
        {
            this.botResponseSender = botResponseSender;
            this.handlers = handlers;
            this.userName = userName;
            this.userID = userID;
            this.firstName = firstName;
        }

        internal Update GetMessageUpdate(string text)
        {
            return new Update()
            {
                Message = new Message()
                {
                    Text = text,
                    From = new User()
                    {
                        Username = userName.Replace("@", string.Empty),
                        Id = userID,
                        FirstName = firstName,
                    },
                    Chat = new Chat()
                    {
                        Id = 1,
                    },
                }
            };
        }

        internal Update GetCallBackUpdate(string data)
        {
            return new Update()
            {
                CallbackQuery = new CallbackQuery()
                {
                    Data = data,
                    From = new User()
                    {
                        Username = userName.Replace("@", string.Empty),
                        Id = userID,
                        FirstName = firstName,
                    },
                    Message = new Message()
                    {
                        Chat = new Chat()
                        {
                            Id = 1,
                        },
                    }
                }
            };
        }

        internal async Task<List<HandlerResult>> StartProcess(Update update, CancellationToken cancellationToken)
        {
            HandlerResult handlerResult = new HandlerResult()
            {
                Update = update,
                HasAccess = true, // Устанавливаем true, считая любое первоначальное обращение разрешённым (чтобы первый раз пройти условие на .HasAccess) 
            };

            List<HandlerResult> handlerResults = new List<HandlerResult>();

            foreach (CoreHandler handler in handlers)
            {
                if (handlerResult.HasAccess)
                {
                    HandlerResult newHandlerResult = await handler.Process(handlerResult, cancellationToken);
                    handlerResults.Add(newHandlerResult);
                    handlerResult = newHandlerResult;

                    if (handlerResult.StopBot)
                        true.Should().BeFalse("Нереализуему в тестах");

                    // Перепроверяем доступ, т.к. во время работы очередного обработчика доступ мог быть отозван
                    if (handlerResult is AuthorizedHandlerResult authorizedHandlerResult && authorizedHandlerResult.InformationSet != null && handlerResult.HasAccess)
                    {
                        await botResponseSender.SendResponse(authorizedHandlerResult.ShortUpdateInfo.ChatId, authorizedHandlerResult.InformationSet, authorizedHandlerResult.CurrentUserContext, cancellationToken);

                        if (!handlerResult.IsNeedContinue)
                            return handlerResults;
                    }
                }
            }

            return handlerResults;
        }

        internal IEnumerable<InlineKeyboardButton> GetCallBackRoutingToNextActFromVerticalButtons(UserContext userContext,
                                                   (ButtonsSet buttonsSet, ButtonsSet backButtonsSet) buttonsSets,
                                                   Dictionary<string, string>? additionalParameters)
        {
            InlineButtonsProvider buttonsProvider = new InlineButtonsProvider(userContext);
            InlineKeyboardMarkup buttonsMarkup = (InlineKeyboardMarkup)buttonsProvider.GetInlineButtons(buttonsSets, additionalParameters);

            IEnumerable<InlineKeyboardButton>? buttons = buttonsMarkup.InlineKeyboard.SelectMany(x => x);
            buttons.Should().NotBeNull();

            return buttons;
        }
    }
}