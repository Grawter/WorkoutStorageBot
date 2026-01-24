using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Telegram.Bot.Types.Enums;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageModels.Entities.Core.Logging;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class AdminCH : CallBackCH
    {
        private AdminRepository AdminRepository { get; }
        private LogsRepository LogsRepository { get; }

        internal AdminCH(CommandHandlerTools commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        {
            AdminRepository = GetRequiredRepository<AdminRepository>();
            LogsRepository = GetRequiredRepository<LogsRepository>();
        }

        internal override async Task<IInformationSet> GetInformationSet()
        {
            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Admin":
                    informationSet = AdminCommand();
                    break;

                case "Logs":
                    informationSet = LogsCommand();
                    break;

                case "ShowLastLog":
                    informationSet = await ShowLastLogCommand();
                    break;

                case "ShowLastExceptionLogs":
                    informationSet = await ShowLastExceptionLogsCommand();
                    break;

                case "FindLogByID":
                    informationSet = FindLogByIDCommand(isEventID: false);
                    break;

                case "FindLogByEventID":
                    informationSet = FindLogByIDCommand(isEventID: true);
                    break;

                case "ShowStartConfiguration":
                    informationSet = ShowStartConfigurationCommand();
                    break;

                case "ChangeLimitsMods":
                    informationSet = ChangeLimitsModsCommand();
                    break;

                case "ChangeWhiteListMode":
                    informationSet = ChangeWhiteListModeCommand();
                    break;

                case "AdminUsers":
                    informationSet = AdminUsersCommand();
                    break;

                case "ShowCountActiveSessions":
                    informationSet = ShowCountActiveSessionsCommand();
                    break;

                case "SendMessageToUser":
                    informationSet = SendMessageToUsersCommand(1);
                    break;

                case "SendMessagesToActiveUsers":
                    informationSet = SendMessageToUsersCommand(2);
                    break;

                case "SendMessagesToAllUsers":
                    informationSet =  SendMessageToUsersCommand(3);
                    break;

                case "ChangeUserState":
                    informationSet = ChangeUserStateCommand();
                    break;

                case "RemoveUser":
                    informationSet = RemoveUserCommand();
                    break;

                case "DisableBot":
                    informationSet = DisableBotCommand();
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            CheckInformationSet(informationSet);

            return informationSet;
        }

        private IInformationSet AdminCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet LogsCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ShowLastLogCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            Log? lastLog = await LogsRepository.GetLogs(1).FirstOrDefaultAsync();

            if (lastLog == null)
                responseTextBuilder = new ResponseTextBuilder("Логов не найдено", "Выберите интересующее действие");
            else
            {
                string logStr = LogFormatter.ConvertLogToStr(lastLog);

                responseTextBuilder = new ResponseTextBuilder("Последний лог:", logStr, "Выберите интересующее действие");
            }

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets, ParseMode.None);

            return informationSet;
        }

        private async Task<IInformationSet> ShowLastExceptionLogsCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            Log? lastErrorLog = await LogsRepository.GetLogs(LogLevel.Error.ToString(), 1).FirstOrDefaultAsync();
            Log? lastCriticalLog = await LogsRepository.GetLogs(LogLevel.Critical.ToString(), 1).FirstOrDefaultAsync();

            List<Log?> exceptionLogs = new List<Log?>() { lastErrorLog, lastCriticalLog };

            bool isNotFirstStr = false;
            StringBuilder sb = new StringBuilder();
            foreach (Log? exceptionLog in exceptionLogs)
            {
                if (exceptionLog != null)
                {
                    if (isNotFirstStr)
                        sb.AppendLine("======================");

                    string logStr = LogFormatter.ConvertLogToStr(exceptionLog, 1000);

                    sb.AppendLine(logStr);

                    isNotFirstStr = true;
                }
            }

            if (sb.Length < 1)
                sb.AppendLine("Логов не найдено");

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Последние ошибочные логи:", sb.ToString(), "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets, ParseMode.None);

            return informationSet;
        }

        private IInformationSet FindLogByIDCommand(bool isEventID)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            string identifierType = isEventID
                             ? "eventId"
                             : "Id";

            this.CurrentUserContext.Navigation.SetMessageNavigationTarget(isEventID
                                                                            ? MessageNavigationTarget.FindLogByEventID
                                                                            : MessageNavigationTarget.FindLogByID);

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"Введите {identifierType} интересующего лога:");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.AdminLogs);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ShowStartConfigurationCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(@$"Текущая настройка:
{AdminRepository.GetSafeConfigurationData()}", "Выберите интересующее действие");

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeLimitsModsCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ILogger logger = this.CreateLogger<AdminCH>();

            this.CurrentUserContext.LimitsManager.ChangeLimitsMode(logger);

            ResponseTextBuilder responseTextBuilder =
                new ResponseTextBuilder($"Режим использования лимитов переключён в: {this.CurrentUserContext.LimitsManager.IsEnableLimit.ToString().AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeWhiteListModeCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            AdminRepository.ChangeWhiteListMode();

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"Белый список включён: {AdminRepository.WhiteListIsEnable.ToString().AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AdminUsersCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ShowCountActiveSessionsCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            int countActiveUserContext = this.ContextKeeper.Count;

            ResponseTextBuilder responseTextBuilder = 
                new ResponseTextBuilder(@$"[{DateTime.Now.ToString(Consts.CommonConsts.Common.DateTimeFormatHoursFirst)}]: {countActiveUserContext.ToString().AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminUsers, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet SendMessageToUsersCommand(int mode)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextBuilder responseTextBuilder;

            MessageNavigationTarget messageNavigationTarget;

            switch (mode)
            {
                case 1:
                    messageNavigationTarget = MessageNavigationTarget.SendMessageToUser;
                    responseTextBuilder = new ResponseTextBuilder($"Введите [UserId/@userLogin]-[Сообщение] для отправки сообщения пользователю", "Пример: @TestUser-Тест");
                    break;

                case 2:
                    messageNavigationTarget = MessageNavigationTarget.SendMessagesToActiveUsers;
                    responseTextBuilder = new ResponseTextBuilder($"Введите сообщение для отправки его всем пользователям с активной сессией");
                    break;

                case 3:
                    messageNavigationTarget = MessageNavigationTarget.SendMessagesToAllUsers;
                    responseTextBuilder = new ResponseTextBuilder($"Введите сообщение для отправки его всем пользователям из БД");
                    break;

                default:
                    throw new NotImplementedException($"Неожиданный тип отправки сообщения из админки: {mode}");
            }

            this.CurrentUserContext.Navigation.SetMessageNavigationTarget(messageNavigationTarget);

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeUserStateCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeUserState);

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Изменение black/white list у пользователя:", $"Введите [UserID/@TestUser] [bl/wl]");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet RemoveUserCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.DeleteUser);

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Удаление пользователя:", "Внимание, пользователь будет удалён без возможности восстановления!".AddBold(), $"Введите [UserID/@TestUser]");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.AdminUsers);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet DisableBotCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            informationSet = new MessageInformationSet("Бот отключён");

            Task.Run(() => this.CloseApp(TimeSpan.FromSeconds(2)));

            return informationSet;
        }

        private bool AccessDenied([NotNullWhen(true)] out IInformationSet? informationSet)
        {
            informationSet = null;

            if (!this.CurrentUserContext.IsAdmin())
            {
                informationSet = SharedCommonLogicHelper.GetAccessDeniedMessageInformationSet();

                return true;
            }
                
            return false;
        }
    }
}