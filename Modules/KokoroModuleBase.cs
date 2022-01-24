using Discord;
using Discord.Commands;
using Kokoro.Common;
using Kokoro.Database;
using System.Threading.Tasks;

namespace Kokoro.Modules
{
    public abstract class KokoroModuleBase : ModuleBase<SocketCommandContext>
    {
        //The dataAccessLayer of Kokoro
        public readonly DataAccessLayer DataAccessLayer;

        public string Prefix
        {
            get
            {
                if (string.IsNullOrEmpty(_prefix))
                {
                    _prefix = DataAccessLayer.GetPrefix(Context.Guild.Id);
                }
                return _prefix;
            }
        }

        private string _prefix;

        public KokoroModuleBase(DataAccessLayer dataAccessLayer)
        {
            DataAccessLayer = dataAccessLayer;
        }

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
