namespace Kokoro;

using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Kokoro.Database;
using Kokoro.Database.Context;
using Kokoro.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Bot
{
    private static string[] args;

    private static async Task Main()
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 200,
                };

                config.Token = context.Configuration["Token"];
            })
            .UseCommandService((context, config) =>
            {
                config.CaseSensitiveCommands = false;
                config.LogLevel = LogSeverity.Debug;
                config.DefaultRunMode = RunMode.Sync;
            })
            .UseInteractionService((context, config) =>
            {
                config.LogLevel = LogSeverity.Debug;
                config.UseCompiledLambda = true;
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<CommandHandler>();
                services.AddHostedService<InteractionHandler>();
                services.AddHostedService<BotStatusService>();
                services.AddHostedService<LongRunningService>();
                services.AddDbContextFactory<KokoroDBContext>(options => 
                options.UseMySql(
                    context.Configuration.GetConnectionString("default"),
                    new MySqlServerVersion(new Version(8, 0, 28))))
                .AddSingleton<DataAccessLayer>();
            })
            .Build();

        await builder.RunAsync();
    }
}
