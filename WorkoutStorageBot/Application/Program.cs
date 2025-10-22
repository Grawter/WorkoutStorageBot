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
using WorkoutStorageBot.BusinessLogic.GlobalContext;
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

                    serviceCollection.AddSingleton<ConfigurationData>(configurationData);

                    serviceCollection.AddLogging(builder =>
                    {
                        //builder.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning); // выключение логов Microsoft.Hosting.Lifetime
                        builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // выключение логирование SQL-команд EF Core
                    });

                    serviceCollection.AddDbContext<EntityContext>(options =>
                    options.UseSqlite(configurationData.DB.ConnectionString)
                           .EnableSensitiveDataLogging(false));

                    serviceCollection.AddScoped<ICustomLoggerFactory, CustomLoggerFactory>();

                    serviceCollection.AddSingleton<IContextKeeper>(GetContextKeeper(configurationData.Bot.IsNeedCacheContext));

                    serviceCollection.AddSingleton<ITelegramBotClient, TelegramBotClient>(sp => new TelegramBotClient(configurationData.Bot.Token));
                    serviceCollection.AddSingleton<IBotResponseSender, BotResponseSender>();

                    serviceCollection.AddSingleton<BotListener>();
                })
                .Build();

            BotListener botListener = host.Services.GetRequiredService<BotListener>();

            await botListener.StartListen(cancellationToken);

            await host.RunAsync(cancellationToken.Token);
        }

        private static ConfigurationData GetConfigurationData()
            => ConfigurationManager.GetConfiguration("./Application/StartConfiguration/appsettings.json");

        private static IContextKeeper GetContextKeeper(bool isNeedCacheMode)
        {
            if (isNeedCacheMode)
                return new MemoryCacheAdapterContextKeeper();
            else
                return new DictionaryAdapterContextKeeper();
        }
    }
}