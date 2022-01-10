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
using Saer.Services;

[RequireUserPermission(GuildPermission.Administrator)]
internal class ModerationModule : General
{
    public ModerationModule(IHost host, ILogger<General> logger) : base(host, logger)
    {
    }

    #region Ban
    [Command("ban")]
    [Summary("Ban a user")]
    public async Task BanMembersAsync(SocketGuildUser socketGuildUser, [Remainder] string reason)
    {
        if (Context.Guild.GetBanAsync(socketGuildUser) == null)
        {
            await Context.Guild.AddBanAsync(socketGuildUser, 0, reason);
            await ReplyAsync($"{socketGuildUser.Username}#{socketGuildUser.Discriminator} Has been Banned! Reason: {reason}");
        }
        else
        {
            await ReplyAsync($"{socketGuildUser.Username}#{socketGuildUser.Discriminator} Has already been Banned!");
        }
    }
    #endregion


    private static LogLevel GetLogLevel(LogSeverity severity)
        => (LogLevel)Math.Abs((int)severity - 5);
}

[Group("status")]
[RequireOwner]
public class StatusModule : ModuleBase<SocketCommandContext>
{
    [Command("set")]
    [Summary("Set a new status")]
    public async Task SetStatusAsync([Remainder] string newStatus)
    {
        LongRunningService.updateStatus = newStatus;
        await ReplyAsync("new status is set");
    }

    [Command("online")]
    [Summary("Set a new activity status")]
    public async Task SetOnlineAsync()
    {
        await Context.Client.SetStatusAsync(UserStatus.Online);
        await ReplyAsync("new status is set");
    }

    [Command("away")]
    [Alias("afk")]
    [Summary("Set a new activity status")]
    public async Task SetAwayAsync()
    {
        await Context.Client.SetStatusAsync(UserStatus.AFK);
        await ReplyAsync("new status is set");
    }
}
