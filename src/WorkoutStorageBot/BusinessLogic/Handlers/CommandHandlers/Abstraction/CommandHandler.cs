using Microsoft.Extensions.Logging;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction
{
    internal abstract class CommandHandler
    {
        protected EntityContext Db { get; }
        private CoreHandler ParentHandler { get; }
        protected UserContext CurrentUserContext { get; }

        protected IContextKeeper ContextKeeper { get; }

        internal CommandHandler(CommandHandlerTools commandHandlerTools)
        {
            this.ParentHandler = commandHandlerTools.ParentHandler;

            this.Db = ParentHandler.CoreTools.Db;

            this.CurrentUserContext = commandHandlerTools.CurrentUserContext;

            this.ContextKeeper = this.ParentHandler.CoreManager.ContextKeeper;
        }

        internal abstract Task<IInformationSet> GetInformationSet();

        internal ILogger CreateLogger<T>()
            => this.ParentHandler.CoreTools.LoggerFactory.CreateLogger<T>();

        internal string? GetBotDescription()
            => this.ParentHandler.CoreTools.ConfigurationData.AboutBotText;

        protected T GetRequiredRepository<T>() where T : CoreRepository
            => this.ParentHandler.CoreManager.GetRequiredRepository<T>();

        protected async Task SimpleSendNotification(long chatId, string message)
            => await this.ParentHandler.CoreManager.SimpleSendNotification(chatId, message);

        protected async Task CloseApp(TimeSpan timeSpan)
            => await this.ParentHandler.CoreManager.CloseApp(timeSpan);
    }
}