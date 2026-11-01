using Microsoft.Extensions.Logging;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.Context.Global;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Core.Sender;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;
using WorkoutStorageBot.Model.DTO.InformationSetForSend;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.Abstraction
{
    internal abstract class CommandHandler
    {
        private CoreTools CoreTools { get; }

        protected AuthorizedHandlerResult HandlerResult { get; }

        protected EntityContext Db { get; }
        
        protected UserContext CurrentUserContext { get; }

        protected IContextKeeper ContextKeeper { get; }

        protected RepositoriesStore RepositoriesHub { get; }

        protected IBotResponseSender BotResponseSender { get; }

        internal CommandHandler(CommandHandlerTools commandHandlerTools)
        {
            this.CoreTools = commandHandlerTools.CoreTools;

            this.HandlerResult = commandHandlerTools.HandlerResult;

            this.Db = commandHandlerTools.CoreTools.Db;

            this.CurrentUserContext = commandHandlerTools.HandlerResult.CurrentUserContext;

            this.ContextKeeper = commandHandlerTools.CoreTools.ContextKeeper;

            this.RepositoriesHub = commandHandlerTools.RepositoriesStore;

            this.BotResponseSender = commandHandlerTools.CoreTools.BotResponseSender;
        }

        internal virtual async Task Process(CancellationToken cancellationToken)
        {
            this.HandlerResult.IsNeedSendResponse = true;
            this.HandlerResult.InformationSet = await GetInformationSet(cancellationToken);
        }

        protected abstract Task<IInformationSet> GetInformationSet(CancellationToken cancellationToken);

        protected ILogger CreateLogger<T>()
            => this.CoreTools.LoggerFactory.CreateLogger<T>();

        protected string? GetBotDescription()
            => this.CoreTools.ConfigurationData.AboutBotText;

        protected ConfigurationData GetConfigurationData()
           => this.CoreTools.ConfigurationData;
    }
}