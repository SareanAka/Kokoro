namespace Saer.Modules;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saer.Common;
using Saer.Services;

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
        _logger.LogInformation("User {user} used the ping command!", Context.User.Username);
        await ReplyAsync("Pong!");
    }
    #endregion

    #region Status
    [Command("status")]
    [Summary("Check the current status")]
    public async Task StatusAsync()
    {
        await ReplyAsync(LongRunningService.updateStatus);
    }
    #endregion

    #region User Info
    [Command("userinfo")]
    [Summary("Get info on a user")]
    public async Task InfoAsync(SocketGuildUser socketGuildUser = null)
    {
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

    #region Mute
    [Command("mute")]
    [Summary("Mute a user")]
    public async Task MuteMembersAsync(SocketGuildUser socketGuildUser, [Remainder] string reason)
    {
        if (socketGuildUser.Roles.Any(r => r.Id == 930118743332384878))
        {
            await ReplyAsync($"**{socketGuildUser.Username}#{socketGuildUser.Discriminator}** Has already been Muted!");
        }
        else
        {
            await socketGuildUser.AddRoleAsync(930118743332384878);
            await ReplyAsync($"**{socketGuildUser.Username}#{socketGuildUser.Discriminator}** Has been Muted. Reason: {reason}");
        }

    }
    #endregion

    private static LogLevel GetLogLevel(LogSeverity severity)
        => (LogLevel)Math.Abs((int)severity - 5);
}

[Group("karaoke")]
public class KaraokeModule : ModuleBase<SocketCommandContext>
{
    #region Join Queue
    [Command("join")]
    [Summary("Join the Karaoke queue")]
    public async Task JoinQueue(SocketGuildUser socketGuildUser = null)
    {
        if (Context.Channel.Id != 930125947762520066)
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

        var embedList = new KoyoriEmbedBuilder()
            .WithAuthor("Karaoke Queue")
            .WithDescription(combindedString);

        if (KaraokeList.Users[0] != null)
        {
            embedList.WithTitle($"Current singer: {KaraokeList.Users[0]}");
        }
        else
        {
            embedList.WithTitle($"The Queue is empty");
        }

        var buildEmbed = embedList.Build();

        await ReplyAsync($"{socketGuildUser.Username} has been added to the queue");
        await Context.Channel.SendMessageAsync(embed: buildEmbed);
    }
    #endregion

    #region Remove from Queue
    [Command("remove")]
    [Summary("Remove someone from the karaoke queue")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    public async Task RemoveFromQueue(SocketGuildUser socketGuildUser = null)
    {
        if (Context.Channel.Id != 930125947762520066)
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

        var embedList = new KoyoriEmbedBuilder()
            .WithAuthor("Karaoke Queue");

        string combindedString;

        if (KaraokeList.Users.Count > 1)
        {
            combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
            embedList.WithDescription(combindedString);
        }

        if (KaraokeList.Users.Count > 0)
        {
            embedList.WithTitle($"Current singer: {KaraokeList.Users[0]}");
        }
        else
        {
            embedList.WithTitle($"The Queue is empty");
        }

        var buildEmbed = embedList.Build();

        await ReplyAsync($"{socketGuildUser.Username} has been removed from the queue");
        await Context.Channel.SendMessageAsync(embed: buildEmbed);
    }
    #endregion

    #region Leave Queue
    [Command("leave")]
    [Summary("Leave from the karaoke queue")]
    public async Task LeaveQueue()
    {
        if (Context.Channel.Id != 930125947762520066)
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

        var embedList = new KoyoriEmbedBuilder()
            .WithAuthor("Karaoke Queue");

        string combindedString;
        if (KaraokeList.Users.Count > 1)
        {
            combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
            embedList.WithDescription(combindedString);
        }

        if (KaraokeList.Users.Count > 0)
        {
            embedList.WithTitle($"Current singer: {KaraokeList.Users[0]}");
        }
        else
        {
            embedList.WithTitle($"The Queue is empty");
        }

        var buildEmbed = embedList.Build();

        await ReplyAsync($"You have left the Queue");
        await Context.Channel.SendMessageAsync(embed: buildEmbed);
    }
    #endregion

    #region Display Queue
    [Command("queue")]
    [Alias("q")]
    [Summary("Remove someone from the karaoke queue")]
    public async Task DisplayQueue()
    {
        if (Context.Channel.Id != 930125947762520066)
        {
            return;
        }

        var embedList = new KoyoriEmbedBuilder()
            .WithAuthor("Karaoke Queue");

        string combindedString;
        if (KaraokeList.Users.Count > 1)
        {
            combindedString = string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1));
            embedList.WithDescription(combindedString);
        }

        if (KaraokeList.Users.Count > 0)
        {
            embedList.WithTitle($"Current singer: {KaraokeList.Users[0]}");
        }
        else
        {
            embedList.WithTitle($"The Queue is empty");
        }

        var buildEmbed = embedList.Build();

        await Context.Channel.SendMessageAsync(embed: buildEmbed);
    }
    #endregion
}