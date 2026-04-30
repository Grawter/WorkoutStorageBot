using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Handlers.Abstraction;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction
{
    internal abstract class CommandHandler
    {
        private CoreHandler ParentHandler { get; }

        protected EntityContext Db { get; }
        
        protected UserContext CurrentUserContext { get; }

        protected IContextKeeper ContextKeeper { get; }

        protected RepositoriesStore RepositoriesHub { get; }

        protected IBotResponseSender BotResponseSender { get; }

        internal CommandHandler(CommandHandlerTools commandHandlerTools)
        {
            this.ParentHandler = commandHandlerTools.ParentHandler;

            this.Db = this.ParentHandler.CoreTools.Db;

            this.CurrentUserContext = commandHandlerTools.CurrentUserContext;

            this.ContextKeeper = this.ParentHandler.CoreTools.ContextKeeper;

            this.RepositoriesHub = this.ParentHandler.RepositoriesHub;

            this.BotResponseSender = this.ParentHandler.CoreTools.BotResponseSender;
        }

        internal abstract Task<IInformationSet> GetInformationSet();

        protected ILogger CreateLogger<T>()
            => this.ParentHandler.CoreTools.LoggerFactory.CreateLogger<T>();

        protected string? GetBotDescription()
            => this.ParentHandler.CoreTools.ConfigurationData.AboutBotText;

        protected ConfigurationData GetConfigurationData()
           => this.ParentHandler.CoreTools.ConfigurationData;

        protected async Task CloseApp(TimeSpan timeSpan)
         => await this.ParentHandler.CloseApp(timeSpan);
    }
}