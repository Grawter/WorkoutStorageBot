#region using

using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Buttons;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.BusinessLogic.StepStore;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.Logger;
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers
{
    internal class PrimaryUpdateHandler
    {
        private readonly ILogger logger;
        private readonly ITelegramBotClient botClient;
        private readonly EntityContext db;
        private readonly Dictionary<long, UserContext> contextStore;
        internal bool WhiteList { get; set; }
        internal bool IsNewContext { get; private set; }
        internal bool HasAccess { get; private set; }

        internal PrimaryUpdateHandler(ILogger logger, ITelegramBotClient botClient, EntityContext db)
        {
            this.logger = logger;
            this.botClient = botClient;
            this.db = db;
            contextStore = new();
        }

        internal void Execute(Update update, out UserContext? currentUserContext)
        {
            if (update != null)
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        ProcessExpectedUpdate(update, out currentUserContext);
                        return;

                    case UpdateType.CallbackQuery:
                        ProcessExpectedUpdate(update, out currentUserContext);
                        return;

                    case UpdateType.EditedMessage:
                        ProcessUnexpectedUpdateType(update.EditedMessage.From, update.Type, update.EditedMessage.Chat.Id);
                        break;

                    case UpdateType.ChannelPost:
                        ProcessUnexpectedUpdateType(update.ChannelPost.From, update.Type, update.ChannelPost.Chat.Id);
                        break;

                    case UpdateType.EditedChannelPost:
                        ProcessUnexpectedUpdateType(update.EditedChannelPost.From, update.Type, update.EditedChannelPost.Chat.Id);
                        break;

                    case UpdateType.MyChatMember:
                        ProcessUnexpectedUpdateType(update.MyChatMember.From, update.Type, update.MyChatMember.Chat.Id);
                        break;

                    case UpdateType.ChatMember:
                        ProcessUnexpectedUpdateType(update.ChatMember.From, update.Type, update.ChatMember.Chat.Id);
                        break;

                    case UpdateType.ChatJoinRequest:
                        ProcessUnexpectedUpdateType(update.ChatJoinRequest.From, update.Type, update.ChatJoinRequest.Chat.Id);
                        break;

                    default:
                        logger.WriteLog($"Неожиданный update.type: {update.Type}", LogType.Anomany);
                        break;
                }
            }

            currentUserContext = null;
        }

        internal void DeleteContext(long id)
        {
            contextStore.Remove(id);
        }

        private void ProcessUnexpectedUpdateType<T>(User user, T updateType, long chatId) where T : Enum
        {
            logger.WriteLog($"От пользователя: @{user.Username} - {user.Id} Данный тип сообщений не обрабатывается: {updateType}", LogType.Warning);
            botClient.SendTextMessageAsync(chatId, $"Данный тип сообщений не обрабатывается: {updateType}");
        }

        private void ProcessExpectedUpdate(Update update, out UserContext? currentUserContext)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message.Type != MessageType.Text)
                    {
                        ProcessUnexpectedUpdateType(update.Message.From, update.Message.Type, update.Message.Chat.Id);
                        break;
                    }

                    LoggingExpectedUpdateType(update.Message.From, update.Message.Text, update.Type);
                    ProcessExpectedUpdateType(update.Message.From, out currentUserContext);

                    if (IsNewContext && HasAccess)
                        DirectIfIsNewContext(update.Message.From.Id, currentUserContext);

                    return;

                case UpdateType.CallbackQuery:
                    LoggingExpectedUpdateType(update.CallbackQuery.From, update.CallbackQuery.Data, update.Type);
                    ProcessExpectedUpdateType(update.CallbackQuery.From, out currentUserContext);

                    if (IsNewContext && HasAccess)
                        DirectIfIsNewContext(update.CallbackQuery.From.Id, in currentUserContext);

                    return;
                default:
                    throw new NotImplementedException($"Неожиданный update.type: {update.Type}");
            }

            currentUserContext = null;
        }

        private void LoggingExpectedUpdateType(User user, string message, UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateType.Message:
                    logger.WriteLog($"От пользователя: @{user.Username} - {user.Id} Текст: {message}");
                    break;

                case UpdateType.CallbackQuery:
                    logger.WriteLog($"От пользователя: @{user.Username} - {user.Id} CallbackQuery: {message}");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный updateType: {updateType}");
            }
        }

        private void ProcessExpectedUpdateType(User user, out UserContext? currentUserContext)
        {
            if (!contextStore.ContainsKey(user.Id))
            {
                AddNewContext(user, out currentUserContext);
                IsNewContext = true;
            }
            else
            {
                currentUserContext = contextStore.GetValueOrDefault(user.Id);

                if (!UserHaveAccess(currentUserContext.UserInformation))
                    return;

                IsNewContext = false;
            }
        }

        private void AddNewContext(User user, out UserContext? currentUserContext)
        {
            var currentUser = db.UsersInformation
                    .Include(u => u.Cycles)
                        .ThenInclude(c => c.Days)
                            .ThenInclude(d => d.Exercises)
                    .FirstOrDefault(u => u.UserId == user.Id);

            if (currentUser != null && !UserHaveAccess(currentUser))
            {
                currentUserContext = null;
                return;
            }
            else if (currentUser == null)
            {
                currentUser = CreateNewUser(user);
                HasAccess = true;
            }

            currentUserContext = new UserContext(currentUser);
            contextStore.Add(user.Id, currentUserContext);
        }

        private bool UserHaveAccess(UserInformation user)
        {
            if (user.BlackList)
                return HasAccess = false;

            if (WhiteList)
            {
                if (user.WhiteList)
                    return HasAccess = true;

                return HasAccess = false;
            }

            return HasAccess = true;
        }

        private UserInformation CreateNewUser(User user)
        {
            var newUser = new UserInformation { UserId = user.Id, Firstname = user.FirstName, Username = "@" + user.Username, WhiteList = false, BlackList = false };
            db.UsersInformation.Add(newUser);
            db.SaveChanges();

            return newUser;
        }

        private void DirectIfIsNewContext(long chatId, in UserContext currentUserContext)
        {
            bool hasCycle;
            hasCycle = currentUserContext.ActiveCycle == null ? false : true;
            DirectToStart(chatId, currentUserContext, hasCycle);
        }

        private void DirectToStart(long chatId, in UserContext currentUserContext, bool hasCycle)
        {
            string message;
            var buttons = new InlineButtons(currentUserContext);
            ButtonsSet buttonsSet;

            if (hasCycle)
            {
                var mainStep = StepStorage.GetMainStep();
                message = new ResponseConverter("Информация о предыдущей сессии не была найдена", mainStep.Message).Convert();
                buttonsSet = mainStep.ButtonsSet;
            }
            else
            {
                currentUserContext.Navigation.QueryFrom = QueryFrom.Start;
                message = "Начнём";
                buttonsSet = ButtonsSet.AddCycle;
            }

            botClient.SendTextMessageAsync(chatId,
                                               message,
                                               replyMarkup: buttons.GetInlineButtons(buttonsSet));
        }
    }
}