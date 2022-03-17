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
		private DropUtil _drop;
		private IServiceProvider _services;
		private CommandHandler _handler;

		const int MINUTE = 60000;

		private int FluxCount = 0;
		private int DropCount = 0;

		Random rand = new();
		private Timer Tick = new(10 * MINUTE);

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
			_drop = _services.GetService<DropUtil>();

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

		private void TickEvent(object source, ElapsedEventArgs e)
		{
			FluxCount -= rand.Next(5, 25);
			DropCount -= rand.Next(2, 10);
		}

		private void updateFlux()
		{
			FluxCount = rand.Next(50, 150);
		}

		private void updateDrop()
		{
			DropCount = rand.Next(25, 100);
		}

		public async Task ClientReady()
		{

			_handler = new CommandHandler(_services);

			await _handler.InstallCommandsAsync();

			updateFlux();
			updateDrop();

			Console.WriteLine("[STARTUP] CollectibleBot Online!");
		}

		public async Task MessageEvent(SocketMessage message)
		{ 
			if (message.Channel is SocketDMChannel || message.Author.IsBot) return;

			Console.WriteLine($"Recieved Message!\n\tFrom: {message.Author.Username}\n\t\t{message.Content}");

			SocketGuildChannel channel = message.Channel as SocketGuildChannel;
			string guildId = channel.Guild.Id.ToString();

			await _util.checkDb(guildId, message.Author.Id.ToString());

			if (message.Content == "!dropadmin" && message.Author.Id == 244905511969882112) DropCount = 0;
			if (message.Content == "!fluxadmin" && message.Author.Id == 244905511969882112) FluxCount = 0;

			if (FluxCount <= 0)
			{
				Console.WriteLine("Running Flux");
				List<Market> markets = _util.getMarkets(guildId);

				Console.WriteLine("Applying Updates");
				foreach (Market market in markets)
				{
					Collectible c = _util.findItem(market.name, guildId);
					Console.WriteLine($"{c.Name} | ${c.Price}");
					market.price = (double) Decimal.Round((decimal) c.generatePrice(), 2);
					Console.WriteLine($"{market.name} | ${market.price}");
				}

				Console.WriteLine("Saving Changes");
				await _context.SaveChangesAsync();

				Console.WriteLine("Updating Tick");
				updateFlux();
			}
			else FluxCount--;

			if (DropCount <= 0)
			{
				// Rolls a rarity to try to make the pool smaller
				int rarity = _drop.rollRarity();
				List<Collectible> pool = _util.findItems(rarity, guildId);

				Random rand = new();
				int chance = rand.Next(0, pool.Count);
				Collectible dropped = pool[chance];

				_drop.generateItem(dropped);
				_drop.dropTimer.Start();

				await channel.Guild.SystemChannel.SendMessageAsync(embed: _drop.dropEmbed(channel.Guild).Build(), components: _drop.claimComponents().Build());

				updateDrop();
			}
			else DropCount--;

			Console.WriteLine($"FluxCount: {FluxCount}");
			Console.WriteLine($"DropCount: {DropCount}");

		}

		public Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
