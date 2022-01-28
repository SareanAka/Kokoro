namespace Kokoro.Modules;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kokoro.Database;
using Kokoro.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ModerationModule : KokoroModuleBase
{
    public ModerationModule(DataAccessLayer dataAccessLayer) : base(dataAccessLayer)
    {

    }

    #region Ban
    [Command("ban")]
    [Summary("Ban a user")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task BanMembersAsync(IGuildUser socketGuildUser = null, [Remainder] string reason = null)
    {
        if (socketGuildUser == null)
        {
            await ReplyAsync("Please specify a user.");
            return;
        }
        if (reason == null) reason = "No reason specified.";
        

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

    #region Mute
    [Command("mute")]
    [Summary("Mute a user")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
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
