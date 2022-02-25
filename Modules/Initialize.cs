using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dotenv.net;
using dotenv.net.Utilities;

namespace CollectibleBot.Modules
{
	public class Initialize
	{
		private readonly CommandService _commands;
		private readonly DiscordSocketClient _client;
		
		public Initialize(
			CommandService commands = null,
			DiscordSocketClient client = null)
		{
			var config = new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				MessageCacheSize = 100
			};

			_commands = commands ?? new CommandService();
			_client = client ?? new DiscordSocketClient(config);
		}

		public IServiceProvider BuildServiceProvider() =>
			new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton(_commands)
				.BuildServiceProvider();

		public void LoadEnv()
		{
			DotEnv.Fluent()
						.WithExceptions()
						.WithEnvFiles()
						.WithTrimValues()
						.WithOverwriteExistingVars()
						.WithProbeForEnv(probeLevelsToSearch: 6)
						.Load();
		}
	}
}
