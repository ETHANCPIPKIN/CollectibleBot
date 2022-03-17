using System;
using System.Threading.Tasks;
using dotenv.net;
using dotenv.net.Utilities;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using CollectibleBot.Modules;
using CollectibleBot.Data;
using System.Timers;
using Discord.Interactions;
using CollectibleBot.Modules.Utils;
using CollectibleBot.Data.Models;
using System.Collections.Generic;

namespace CollectibleBot
{
	class Program
	{
		private DiscordSocketClient _client;
		private CommandService _commands;
		private InteractionService _interact;
		private DbUtil _util;
		private BotDb _context;
		private IServiceProvider _services;
		private CommandHandler _handler;

		const int MINUTE = 60000;

		static Task Main(string[] args) => new Program().MainAsync();

		public async Task MainAsync()
		{
			Initialize init = new();

			init.LoadEnv();
			_services = init.BuildServiceProvider();

			_context = _services.GetService<BotDb>();

			var create = await _context.Database.EnsureCreatedAsync();
			Console.WriteLine($"Created: {create}");

			_client = _services.GetService<DiscordSocketClient>();
			_commands = _services.GetService<CommandService>();
			_interact = _services.GetService<InteractionService>();
			_handler = _services.GetService<CommandHandler>();
			_util = _services.GetService<DbUtil>();

			Console.WriteLine("Instantiated services...");

			_client.Ready += ClientReady;
			_client.MessageReceived += MessageEvent;

			Tick.Elapsed += TickEvent;

			var token = EnvReader.GetStringValue("TOKEN");

			Console.WriteLine("Logging in...");
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
			Console.WriteLine("Logged in!");

			await Task.Delay(-1);

		}

		public async Task ClientReady()
		{

			_handler = new CommandHandler(_services);

			await _handler.InstallCommandsAsync();

			Console.WriteLine("[STARTUP] CollectibleBot Online!");
		}

		public async Task MessageEvent(SocketMessage message)
		{ 
			if (message.Channel is SocketDMChannel || message.Author.IsBot) return;

			Console.WriteLine($"Recieved Message!\n\tFrom: {message.Author.Username}\n\t\t{message.Content}");

			SocketGuildChannel channel = message.Channel as SocketGuildChannel;
			string guildId = channel.Guild.Id.ToString();

			await _util.checkDb(guildId, message.Author.Id.ToString());

		}

		public Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
