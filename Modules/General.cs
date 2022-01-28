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
using Kokoro.Database;
using Kokoro.Models;

public class General : KokoroModuleBase
{
    private readonly ILogger<General> _logger;

    // You can inject the host. This is useful if you want to shutdown the host via a command, but be careful with it.
    private readonly IHost _host;


    public General(IHost host, ILogger<General> logger, DataAccessLayer dataAccessLayer)
        : base(dataAccessLayer)
    {
        _host = host;
        _logger = logger;
        Console.WriteLine("");
    }

    #region Ping
    [Command("ping")]
    [Summary("Bot sends a respons")]
    public async Task PingAsync()
    {
        if (Context.Channel.Id != 915308889975697460)
        {
            return;
        }

        _logger.LogInformation($"User {Context.User} used the ping command!", Context.User.Username);
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

    #region prefix
    [Command("prefix")]
    public async Task GetPrefixAsync()
    {
        await ReplyAsync($"My Prefix is {Prefix}.");
    }

    [Command("prefix set")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetPrefixAsync(string prefix = null)
    {
        if (prefix == null)
        {
            await ReplyAsync($"Please provide a new prefix");
            return;
        }
        await DataAccessLayer.SetPrefix(Context.Guild.Id, prefix);
        CommandHandler.Instance.ServerPrefixes[Context.Guild.Id] = prefix;
        await ReplyAsync($"My new Prefix is {prefix}");
    }
    #endregion

    private static LogLevel GetLogLevel(LogSeverity severity)
        => (LogLevel)Math.Abs((int)severity - 5);
}

[Group("karaoke")]
[Alias("k")]
public class KaraokeModule : KokoroModuleBase
{
    private const string AUTHOR = "Karaoke Queue";

    public KaraokeModule(DataAccessLayer dataAccessLayer) : base(dataAccessLayer)
    {
    }

    private async Task EmbedAsync(string message = null)
    {
        var combinedString = KaraokeList.Users.Count > 1 ? string.Join($"\n", KaraokeList.Users.GetRange(1, KaraokeList.Users.Count - 1).Select(x => x.UserName)) : string.Empty;
        var title = KaraokeList.Users.Count > 0 ? $"Current singer: {KaraokeList.Users[0].UserName}" : "The Queue is empty";

        if (!string.IsNullOrEmpty(message)) await ReplyAsync(message);

        await SendEmbedAsync(AUTHOR, title, combinedString);
    }

    [Command("")]
    public async Task EmbedAsync()
    {
        await ReplyAsync("Please provide an argument.");
    }

    #region Add Karaoke Channel
    [Command("channel add")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddKaraokeAsync(IMessageChannel karaokeChannel = null)
    {
        if (karaokeChannel == null)
        {
            await ReplyAsync("Please provide a channel");
            return;
        }
        await DataAccessLayer.AddKaraokeChannel(Context.Guild.Id, karaokeChannel.Id);
        await ReplyAsync($"<#{karaokeChannel.Id}> has set as the karaoke channel.");
    }
    #endregion

    #region Join Queue
    [Command("join")]
    [Summary("Join the Karaoke queue")]
    public async Task JoinQueue(SocketGuildUser socketGuildUser = null)
    {
        if (Context.Channel.Id != await DataAccessLayer.GetKaraokeChannel(Context.Guild.Id))
        {
            return;
        }

        if (socketGuildUser == null)
        {
            socketGuildUser = Context.User as SocketGuildUser;
        }

        if (KaraokeList.Users.Select(user => user.UserId).Contains(socketGuildUser.Id))
        {
            await ReplyAsync($"{socketGuildUser.Username} is already in the queue");
            return;
        }

        KaraokeList.Users.Add(new UserModel(socketGuildUser.Id, socketGuildUser.Username));

        await EmbedAsync($"{socketGuildUser.Username} has been added to the queue");
    }
    #endregion

    #region Remove from Queue
    
    [Command("remove")]
    [Summary("Remove someone from the karaoke queue")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    public async Task RemoveFromQueue(SocketGuildUser socketGuildUser = null)
    {
        if (Context.Channel.Id != await DataAccessLayer.GetKaraokeChannel(Context.Guild.Id))
        {
            return;
        }

        if (socketGuildUser == null)
        {
            await ReplyAsync($"Please provide a name");
            return;
        }

        var karaokeUser = KaraokeList.Users.SingleOrDefault(user => user.UserId == socketGuildUser.Id);
        if (karaokeUser != null)
        {
            KaraokeList.Users.Remove(karaokeUser);
        }
        else
        {
            await ReplyAsync($"That person is not in the queue");
            return;
        }
        await EmbedAsync($"{socketGuildUser.Username} has been removed from the queue");
    }
    #endregion

    #region Leave Queue
    [Command("leave")]
    [Summary("Leave from the karaoke queue")]
    public async Task LeaveQueue()
    {
        if (Context.Channel.Id != await DataAccessLayer.GetKaraokeChannel(Context.Guild.Id))
        {
            return;
        }

        SocketGuildUser socketGuildUser = Context.User as SocketGuildUser;

        var karaokeUser = KaraokeList.Users.SingleOrDefault(user => user.UserId == socketGuildUser.Id);
        if (karaokeUser != null)
        {
            KaraokeList.Users.Remove(karaokeUser);
        }
        else
        {
            await ReplyAsync($"You are not in the queue!");
            return;
        }
        await EmbedAsync("You have left the Queue");
    }
    #endregion

    #region Display Queue
    [Command("queue")]
    [Alias("q")]
    [Summary("Remove someone from the karaoke queue")]
    public async Task DisplayQueue()
    {
        if (Context.Channel.Id != await DataAccessLayer.GetKaraokeChannel(Context.Guild.Id))
        {
            return;
        }

        await EmbedAsync();
    }
    #endregion

    #region Move Queue
    [Command("next")]
    [Alias("n")]
    [Summary("move the karaoke queue")]
    public async Task NextQueueAsync()
    {
        if (Context.Channel.Id != await DataAccessLayer.GetKaraokeChannel(Context.Guild.Id))
        {
            return;
        }

        UserModel firsUser = KaraokeList.Users.First();

        KaraokeList.Users.Add(firsUser);
        KaraokeList.Users.RemoveAt(0);

        await EmbedAsync($"Next up is: {KaraokeList.Users.First().Mention}");
    }
    
    #endregion

}