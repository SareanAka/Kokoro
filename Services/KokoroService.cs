using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Kokoro.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Kokoro.Services
{
    public abstract class KokoroService : DiscordClientService
    {
        public readonly DiscordSocketClient Client;
        public readonly ILogger<DiscordClientService> Logger;
        public readonly CommandService CommandService;
        public readonly IConfiguration Config;
        public readonly DataAccessLayer DataAccessLayer;

        public KokoroService(DiscordSocketClient client, ILogger<DiscordClientService> logger,
             IConfiguration config, DataAccessLayer dataAccessLayer) : base(client, logger)
        {
            Client = client;
            Logger = logger;
            Config = config;
            DataAccessLayer = dataAccessLayer;
        }
    }
}
