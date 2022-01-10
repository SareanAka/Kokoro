namespace Saer.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class LongRunningService : DiscordClientService
{
    public static string updateStatus = "KoyoKek";
    public static UserStatus setActivityStatus = UserStatus.Online;

    public LongRunningService(DiscordSocketClient client, ILogger<DiscordClientService> logger)
        : base(client, logger)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the client to be ready
        await Client.WaitForReadyAsync(stoppingToken);

        // Start a pumping background service that lasts for the length of host's existence.
        while (!stoppingToken.IsCancellationRequested)
        {
            await Client.SetActivityAsync(new Game(updateStatus));
            await Client.SetStatusAsync(setActivityStatus);
            await Task.Delay(10000, stoppingToken);
        }
    }
}
