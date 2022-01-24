using Discord;
using Discord.Commands;
using Saer.Common;
using System.Threading.Tasks;

namespace Saer.Modules
{
    public abstract class KokoroModuleBase : ModuleBase<SocketCommandContext>
    {

        /// <summary>
        /// Send an embed containing a title and description
        /// </summary>
        /// <param name="title">Title of the embed</param>
        /// <param name="description">Description of the embed</param>
        /// <returns></returns>
        public async Task<IMessage> SendEmbedAsync(string author, string title, string description = null)
        {
            var builder = new KoyoriEmbedBuilder()
                .WithAuthor(author)
                .WithTitle(title)
                .WithDescription(description);
            
            return await Context.Channel.SendMessageAsync(embed: builder.Build());
        }
    }
}
