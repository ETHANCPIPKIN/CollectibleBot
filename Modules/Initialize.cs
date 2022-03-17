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
using CollectibleBot.Data;
using Discord.Interactions;
using Fergun.Interactive;
using CollectibleBot.Modules.Utils;

namespace CollectibleBot.Modules
{
	public class Initialize
	{
		private readonly CommandService _commands;
		private readonly DiscordSocketClient _client;
		private readonly BotDb _ctx;
		private readonly InteractionService _interact;
		private readonly InteractiveService _interactive;
		private readonly DbUtil _util;
		private readonly ItemUtil _item;
		private readonly DropUtil _drop;

		public Initialize(
			CommandService commands = null,
			DiscordSocketClient client = null,
			BotDb ctx = null,
			InteractionService interact = null,
			InteractiveService interactive = null,
			DbUtil util = null,
			ItemUtil item = null,
			DropUtil drop = null)
		{
			var config = new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				MessageCacheSize = 100
			};

			_commands = commands ?? new CommandService();
			_client = client ?? new DiscordSocketClient(config);
			_ctx = ctx ?? new BotDb();
			_interact = interact ?? new InteractionService(_client);
			_interactive = interactive ?? new InteractiveService(_client);
			_util = util ?? new DbUtil(_client, _ctx);
			_item = item ?? new ItemUtil();
			_drop = drop ?? new DropUtil(_ctx);
		}

		public IServiceProvider BuildServiceProvider() =>
			new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton(_commands)
				.AddSingleton(_ctx)
				.AddSingleton(_interact)
				.AddSingleton(_interactive)
				.AddSingleton(_util)
				.AddSingleton(_item)
				.AddSingleton(_drop)
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
