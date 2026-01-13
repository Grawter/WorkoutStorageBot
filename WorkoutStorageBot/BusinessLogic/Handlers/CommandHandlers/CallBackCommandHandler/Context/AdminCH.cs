using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.SharedCommandHandler;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Core.Helpers;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.Entities.Logging;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class AdminCH : CallBackCH
    {
        private ILogger Logger { get; }
        private AdminRepository AdminRepository { get; }
        private LogsRepository LogsRepository { get; }

        internal AdminCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        {
            AdminRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRequiredRepository<AdminRepository>();
            LogsRepository = this.CommandHandlerTools.ParentHandler.CoreManager.GetRequiredRepository<LogsRepository>();

            Logger = CommonHelper.GetIfNotNull(commandHandlerTools.ParentHandler.CoreTools.LoggerFactory).CreateLogger<AdminCH>();
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

            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet LogsCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ShowLastLogCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            Log? lastLog = await LogsRepository.GetLogs(1).FirstOrDefaultAsync();

            if (lastLog == null)
                responseConverter = new ResponseTextConverter("Логов не найдено", "Выберите интересующее действие");
            else
            {
                string logStr = LogFormatter.ConvertLogToStr(lastLog);

                responseConverter = new ResponseTextConverter("Последний лог:", logStr, "Выберите интересующее действие");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

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
            foreach (Log exceptionLog in exceptionLogs)
            {
                if (exceptionLog != null)
                {
                    if (isNotFirstStr)
                        sb.AppendLine("======================");

                    string logStr = LogFormatter.ConvertLogToStr(exceptionLog);

                    sb.AppendLine(logStr);

                    isNotFirstStr = true;
                }
            }

            if (sb.Length < 1)
                sb.AppendLine("Логов не найдено");

            ResponseTextConverter responseConverter = new ResponseTextConverter("Последние ошибочные логи:", sb.ToString(), "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet FindLogByIDCommand(bool isEventID)
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            string identifierType = isEventID
                             ? "eventId"
                             : "Id";

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(isEventID
                                                                                             ? MessageNavigationTarget.FindLogByEventID
                                                                                             : MessageNavigationTarget.FindLogByID);

            ResponseTextConverter responseConverter = new ResponseTextConverter($"Введите {identifierType} интересующего лога:");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.AdminLogs);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ShowStartConfigurationCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            ResponseTextConverter responseConverter = new ResponseTextConverter(@$"Текущая настройка:
{AdminRepository.SaveConfigurationData}", "Выберите интересующее действие");

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeLimitsModsCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            this.CommandHandlerTools.CurrentUserContext.LimitsManager.ChangeLimitsMode();

            ResponseTextConverter responseConverter =
                new ResponseTextConverter($"Режим использования лимитов переключён в: {this.CommandHandlerTools.CurrentUserContext.LimitsManager.IsEnableLimit.ToString().AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            Logger.LogWarning($"Режим использования лимитов переключён в: {this.CommandHandlerTools.CurrentUserContext.LimitsManager.IsEnableLimit.ToString()}");

            return informationSet;
        }

        private IInformationSet ChangeWhiteListModeCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            AdminRepository.ChangeWhiteListMode();

            ResponseTextConverter responseConverter = new ResponseTextConverter($"Белый список включён: {AdminRepository.WhiteListIsEnable.ToString().AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeUserStateCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeUserState);

            ResponseTextConverter responseConverter = new ResponseTextConverter("Изменение black/white list у пользователя:", $"Введите [userID/@userLogin] [bl/wl]");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet RemoveUserCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.DeleteUser);

            ResponseTextConverter responseConverter = new ResponseTextConverter("Удаление пользователя:", "Внимание, пользователь будет удалён без возможности восстановления!".AddBold(), $"Введите [userID/@userLogin]");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.Admin);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet DisableBotCommand()
        {
            if (AccessDenied(out IInformationSet? informationSet))
                return informationSet;

            informationSet = new MessageInformationSet("Бот отключён");

            Task.Run(() => this.CommandHandlerTools.ParentHandler.CoreManager.StopManaging(TimeSpan.FromSeconds(2)));

            return informationSet;
        }

        private bool AccessDenied([NotNullWhen(true)] out IInformationSet? informationSet)
        {
            informationSet = null;

            if (!this.CommandHandlerTools.CurrentUserContext.IsAdmin())
            {
                SharedCH sharedCH = new SharedCH(this.CommandHandlerTools);

                informationSet = sharedCH.GetAccessDeniedMessageInformationSet();

                return true;
            }
                
            return false;
        }
    }
}