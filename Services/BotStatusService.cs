namespace Saer.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class BotStatusService : DiscordClientService
{
    public BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger) : base(client, logger)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string status = LongRunningService.updateStatus;
        await Client.WaitForReadyAsync(stoppingToken);
        Logger.LogInformation("Client is ready!");

        var fileStream = new FileStream("C:/Users/tommy/source/repos/Saer/Saer/Services/Source/KoyoriPet.png", FileMode.Open);
        var image = new Image(fileStream);
        await Client.SetActivityAsync(new Game(status));
        await Client.CurrentUser.ModifyAsync(u => u.Avatar = image);
        await Client.CurrentUser.ModifyAsync(u => u.Username = "Kokoro");
    }
}
