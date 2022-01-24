namespace Kokoro.Modules;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kokoro.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kokoro.Common;

public class General : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<General> _logger;

    // You can inject the host. This is useful if you want to shutdown the host via a command, but be careful with it.
    private readonly IHost _host;

    public General(IHost host, ILogger<General> logger)
    {
        _host = host;
        _logger = logger;
        Console.WriteLine("");
    }

    #region Ping
    [Command("ping")]
    [Alias("pong", "hello")]
    [Summary("Bot sends a respons")]
    public async Task PingAsync()
    {
        if (Context.Channel.Id != 915308889975697460)
        {
            return;
        }

        _logger.LogInformation("User {user} used the ping command!", Context.User.Username);
        await ReplyAsync("Pong!");
    }
    #endregion

    #region Status
    [Command("status")]
    [Summary("Check the current status")]
    public async Task StatusAsync()
    {
        if (Context.Channel.Id != 915308889975697460)
        {
            return;
        }

        await ReplyAsync(LongRunningService.updateStatus);
    }
    #endregion

    #region User Info
    [Command("userinfo")]
    [Summary("Get info on a user")]
    public async Task InfoAsync(SocketGuildUser socketGuildUser = null)
    {
        if (Context.Channel.Id != 915308889975697460)
        {
            return;
        }

        if (socketGuildUser == null)
        {
            socketGuildUser = Context.User as SocketGuildUser;
        }

        var embed = new KoyoriEmbedBuilder()
            .WithTitle($"{socketGuildUser.Username}#{socketGuildUser.Discriminator}")
            .WithThumbnailUrl(socketGuildUser.GetAvatarUrl() ?? socketGuildUser.GetDefaultAvatarUrl())
            .AddField("ID:", socketGuildUser.Id, true)
            .AddField("Name:", socketGuildUser.Nickname, true)
            .AddField("Created on:", $"<t:{socketGuildUser.CreatedAt.ToUnixTimeSeconds()}:F>")
            .WithCurrentTimestamp()
            .Build();

        await ReplyAsync(embed: embed);
    }
    #endregion

    #region Echo
    [Command("echo")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task Echo(IMessageChannel channel, [Remainder] string input)
    {
        ulong c = channel.Id;
        var chnl = Context.Client.GetChannel(c) as IMessageChannel;
        await chnl.SendMessageAsync(input);
        await ReplyAsync($"Message sent to <#{c}>");
    }
    #endregion

    [Command("prefix")]
    public async Task GetPrefix()
    {
        await ReplyAsync($"My Prefix is");
    }

    private static LogLevel GetLogLevel(LogSeverity severity)
        => (LogLevel)Math.Abs((int)severity - 5);
}

[Group("karaoke")]
[Alias("k")]
public class KaraokeModule : KokoroModuleBase
{
    #region Join Queue
    [Command("join")]
    [Summary("Join the Karaoke queue")]
    public async Task JoinQueue(SocketGuildUser socketGuildUser = null)
    {
        if (Context.Channel.Id != 931905280374112317)
        {
            return;
        }

        if (socketGuildUser == null)
        {
            socketGuildUser = Context.User as SocketGuildUser;
        }

        if (KaraokeList.Users.Contains(socketGuildUser.Username))
        {
            await ReplyAsync($"{socketGuildUser.Username} is already in the queue");
            return;
        }

        KaraokeList.Users.Add($"{socketGuildUser.Username}");

        string combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
        string author = "Karaoke Queue";
        string title;


        if (KaraokeList.Users[0] != null)
        {
            title = $"Current singer: {KaraokeList.Users[0]}";
        }
        else
        {
            title = $"The Queue is empty";
        }

        await ReplyAsync($"{socketGuildUser.Username} has been added to the queue");
        await SendEmbedAsync(author, title, combindedString);
    }
    #endregion

    #region Remove from Queue
    [Command("remove")]
    [Summary("Remove someone from the karaoke queue")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    public async Task RemoveFromQueue(SocketGuildUser socketGuildUser = null)
    {
        if (Context.Channel.Id != 931905280374112317)
        {
            return;
        }

        if (socketGuildUser == null)
        {
            await ReplyAsync($"Please provide a name");
            return;
        }

        if (KaraokeList.Users.Contains(socketGuildUser.Username))
        {
            KaraokeList.Users.Remove(socketGuildUser.Username);
        }
        else
        {
            await ReplyAsync($"That person is not in the queue");
            return;
        }


        string combindedString;
        string author = "Karaoke Queue";
        string title;

        if (KaraokeList.Users.Count > 1)
        {
            combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
        }
        else
        {
            combindedString = "";
        }

        if (KaraokeList.Users.Count > 0)
        {
            title = $"Current singer: {KaraokeList.Users[0]}";
        }
        else
        {
            title = $"The Queue is empty";
        }

        await ReplyAsync($"{socketGuildUser.Username} has been removed from the queue");
        await SendEmbedAsync(author, title, combindedString);
    }
    #endregion

    #region Leave Queue
    [Command("leave")]
    [Summary("Leave from the karaoke queue")]
    public async Task LeaveQueue()
    {
        if (Context.Channel.Id != 931905280374112317)
        {
            return;
        }

        SocketGuildUser socketGuildUser = Context.User as SocketGuildUser;

        if (KaraokeList.Users.Contains(socketGuildUser.Username))
        {
            KaraokeList.Users.Remove(socketGuildUser.Username);
        }
        else
        {
            await ReplyAsync($"You are not in the queue!");
            return;
        }

        string combindedString;
        string author = "Karaoke Queue";
        string title;

        if (KaraokeList.Users.Count > 1)
        {
            combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
        }
        else
        {
            combindedString = "";
        }

        if (KaraokeList.Users.Count > 0)
        {
            title = $"Current singer: {KaraokeList.Users[0]}";
        }
        else
        {
            title = $"The Queue is empty";
        }

        await ReplyAsync($"You have left the Queue");
        await SendEmbedAsync(author, title, combindedString);
    }
    #endregion

    #region Display Queue
    [Command("queue")]
    [Alias("q")]
    [Summary("Remove someone from the karaoke queue")]
    public async Task DisplayQueue()
    {
        if (Context.Channel.Id != 931905280374112317)
        {
            return;
        }

        string combindedString;
        string author = "Karaoke Queue";
        string title;

        if (KaraokeList.Users.Count > 1)
        {
            combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
        }
        else
        {
            combindedString = "";
        }

        if (KaraokeList.Users.Count > 0)
        {
            title = $"Current singer: {KaraokeList.Users[0]}";
        }
        else
        {
            title = $"The Queue is empty";
        }

        await SendEmbedAsync(author, title, combindedString);
    }
    #endregion

    #region Move Queue
    [Command("next")]
    [Alias("n")]
    [Summary("move the karaoke queue")]
    public async Task NextQueueAsync()
    {
        if (Context.Channel.Id != 931905280374112317)
        {
            return;
        }

        string firsUser = KaraokeList.Users[0];

        KaraokeList.Users.Add(firsUser);
        KaraokeList.Users.RemoveAt(0);

        string combindedString;
        string author = "Karaoke Queue";
        string title;

        if (KaraokeList.Users.Count > 1)
        {
            combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
        }
        else
        {
            combindedString = "";
        }

        if (KaraokeList.Users.Count > 0)
        {
            title = $"Current singer: {KaraokeList.Users[0]}";
        }
        else
        {
            title = $"The Queue is empty";
        }


        await SendEmbedAsync(author, title, combindedString);
    }
    #endregion

}