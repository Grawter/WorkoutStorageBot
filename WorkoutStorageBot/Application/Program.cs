#region using

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using WorkoutStorageBot.Application.BotTools.Listener;
using WorkoutStorageBot.Application.BotTools.Logging;
using WorkoutStorageBot.Application.BotTools.Sender;
using WorkoutStorageBot.Application.Configuration;
using WorkoutStorageBot.BusinessLogic.CoreRepositories;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers;
using WorkoutStorageBot.Core.Abstraction;
using WorkoutStorageBot.Core.Manager;
using WorkoutStorageBot.Model.AppContext;

#endregion

namespace WorkoutStorageBot.Application
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using CancellationTokenSource cancellationToken = new CancellationTokenSource();

            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, serviceCollection) =>
                {
                    ConfigurationData configurationData = GetConfigurationData();

                    EntityContext db = GetEntityContext(configurationData.DB.ConnectionString);

                    CustomLoggerProvider loggerProvider = new CustomLoggerProvider(db, configurationData);

                    LoggerFactory loggerFactory = new LoggerFactory([loggerProvider]);

                    serviceCollection.AddSingleton<ConfigurationData>(configurationData);
                    serviceCollection.AddSingleton<EntityContext>(db);

                    serviceCollection.AddSingleton<ILoggerProvider, CustomLoggerProvider>(sp => loggerProvider);
                    serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>(sp => loggerFactory);

                    serviceCollection.AddSingleton<ITelegramBotClient, TelegramBotClient>(sp => new TelegramBotClient(configurationData.Bot.Token));
                    serviceCollection.AddSingleton<IBotResponseSender, BotResponseSender>();

                    List<CoreHandler> handlers = HandlersProvider.InitHandlers(db, loggerFactory, configurationData);
                    List<CoreRepository> repositories = RepositoriesProvider.InitRepositories(db, loggerFactory, configurationData);

                    serviceCollection.AddSingleton<CoreManager>(sp => new CoreManager(handlers, repositories, configurationData, sp.GetRequiredService<IBotResponseSender>(), loggerFactory, cancellationToken));

                    serviceCollection.AddSingleton<BotListener>();
                })
                .Build();

            BotListener botListener = host.Services.GetRequiredService<BotListener>();

            await botListener.StartListen();

            await host.RunAsync(cancellationToken.Token);
        }

        private static ConfigurationData GetConfigurationData()
            => ConfigurationManager.GetConfiguration("./Application/StartConfiguration/appsettings.json");

        private static EntityContext GetEntityContext(string connectionString)
        {
            DbContextOptionsBuilder<EntityContext> optionsBuilder = new DbContextOptionsBuilder<EntityContext>();
            DbContextOptions<EntityContext> options = optionsBuilder.UseSqlite(connectionString).Options;
            EntityContext db = new EntityContext(options);

            return db;
        }
    }
}