#region using

using Microsoft.Extensions.Logging;
using System.Text;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.SharedCommandHandler;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.Entities.Logging;
using WorkoutStorageBot.Model.DTO.HandlerData;

#endregion

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

        internal override AdminCH Expectation(params HandlerAction[] handlerActions)
        {
            this.HandlerActions = handlerActions;

            return this;
        }

        internal AdminCH AdminCommand()
        {
            if (AccessDenied())
                return this;

            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH LogsCommand()
        {
            if (AccessDenied())
                return this;

            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH ShowLastLogCommand()
        {
            if (AccessDenied())
                return this;

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.AdminLogs, ButtonsSet.Admin);

            Log? lastLog = LogsRepository.GetLogs(1).FirstOrDefault();

            if (lastLog == null)
                responseConverter = new ResponseTextConverter("Логов не найдено", "Выберите интересующее действие");
            else
            {
                string logStr = LogFormatter.ConvertLogToStr(lastLog);

                responseConverter = new ResponseTextConverter("Последний лог:", logStr, "Выберите интересующее действие");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH ShowLastExceptionLogsCommand()
        {
            if (AccessDenied())
                return this;

            Log? lastErrorLog = LogsRepository.GetLogs(LogLevel.Error.ToString(), 1).FirstOrDefault();
            Log? lastCriticalLog = LogsRepository.GetLogs(LogLevel.Critical.ToString(), 1).FirstOrDefault();

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

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH FindLogByIDCommand(bool isEventID)
        {
            if (AccessDenied())
                return this;

            string identifierType = isEventID
                             ? "eventId"
                             : "Id";

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(isEventID
                                                                                             ? MessageNavigationTarget.FindLogByEventID
                                                                                             : MessageNavigationTarget.FindLogByID);

            ResponseTextConverter responseConverter = new ResponseTextConverter($"Введите {identifierType} интересующего лога:");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.AdminLogs);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH ShowStartConfigurationCommand()
        {
            if (AccessDenied())
                return this;

            ResponseTextConverter responseConverter = new ResponseTextConverter(@$"Текущая настройка:
{AdminRepository.SaveConfigurationData}", "Выберите интересующее действие");

            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH ChangeLimitsModsCommand()
        {
            if (AccessDenied())
                return this;

            this.CommandHandlerTools.CurrentUserContext.LimitsManager.ChangeLimitsMode();

            ResponseTextConverter responseConverter =
                new ResponseTextConverter($"Режим использования лимитов переключён в: {this.CommandHandlerTools.CurrentUserContext.LimitsManager.IsEnableLimit.ToString().AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            Logger.LogWarning($"Режим использования лимитов переключён в: {this.CommandHandlerTools.CurrentUserContext.LimitsManager.IsEnableLimit.ToString()}");

            return this;
        }

        internal AdminCH ChangeWhiteListModeCommand()
        {
            if (AccessDenied())
                return this;

            AdminRepository.ChangeWhiteListMode();

            ResponseTextConverter responseConverter = new ResponseTextConverter($"Белый список включён: {AdminRepository.WhiteListIsEnable.ToString().AddBold()}",
                "Выберите интересующее действие");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Admin, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH ChangeUserStateCommand()
        {
            if (AccessDenied())
                return this;

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeUserState);

            ResponseTextConverter responseConverter = new ResponseTextConverter("Изменение black/white list у пользователя:", $"Введите [userID/@userLogin] [bl/wl]");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.Admin);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH RemoveUserCommand()
        {
            if (AccessDenied())
                return this;

            this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.DeleteUser);

            ResponseTextConverter responseConverter = new ResponseTextConverter("Удаление пользователя:", "Внимание, пользователь будет удалён без возможности восстановления!".AddBold(), $"Введите [userID/@userLogin]");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.Admin);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal AdminCH DisableBotCommand()
        {
            if (AccessDenied())
                return this;

            this.InformationSet = new MessageInformationSet("Бот отключён");

            Task.Run(() => this.CommandHandlerTools.ParentHandler.CoreManager.StopManaging(TimeSpan.FromSeconds(2)));

            return this;
        }

        private bool AccessDenied()
        {
            if (!this.CommandHandlerTools.CurrentUserContext.IsAdmin())
            {
                SharedCH sharedCH = new SharedCH(this.CommandHandlerTools);

                this.InformationSet = sharedCH.GetAccessDeniedMessageInformationSet();
                return true;
            }
                
            return false;
        }
    }
}