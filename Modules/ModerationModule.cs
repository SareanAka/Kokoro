namespace Saer.Modules;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Saer.Services;

[RequireUserPermission(GuildPermission.Administrator)]
internal class ModerationModule : ModuleBase<SocketCommandContext>
{
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

    #region Mute
    [Command("mute")]
    [Summary("Mute a user")]
    public async Task MuteMembersAsync(SocketGuildUser socketGuildUser, [Remainder] string reason)
    {
        if (Context.Guild.GetUser(socketGuildUser.Id).Roles.Any(r => r.Name == "Bonked"))
        {
            await ReplyAsync($"{socketGuildUser.Username}#{socketGuildUser.Discriminator} Has already been Muted!");
        }
        else
        {
            await Context.Guild.GetUser(socketGuildUser.Id).AddRoleAsync(930118743332384878);
            await ReplyAsync($"{socketGuildUser.Username}#{socketGuildUser.Discriminator} Has been Muted! Reason: {reason}");
        }

    }
    #endregion
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
        LongRunningService.setActivityStatus = UserStatus.Online;
        await ReplyAsync("new status is set");
    }

    [Command("away")]
    [Summary("Set a new activity status")]
    public async Task SetAwayAsync()
    {
        LongRunningService.setActivityStatus = UserStatus.AFK;
        await ReplyAsync("new status is set");
    }
}
