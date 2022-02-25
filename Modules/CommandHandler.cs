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

namespace CollectibleBot.Modules
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private IServiceProvider _services;

		public CommandHandler(IServiceProvider services)
		{
			_services = services;

			_client = services.GetService<DiscordSocketClient>();
			_commands = services.GetService<CommandService>();
		}

		public async Task InstallCommandsAsync()
		{
			_client.MessageReceived += HandleCommandAsync;
			await _commands.AddModulesAsync(
				assembly: Assembly.GetEntryAssembly(),
				services: _services);

			Console.WriteLine("[STARTUP] Installed Commands!");
		}

		private async Task HandleCommandAsync(SocketMessage param)
		{
			if (param.Channel is SocketDMChannel ||
				param.Author.IsBot) return;
			Console.WriteLine("[DEBUG] Recieved Message!");

			var msg = param as SocketUserMessage;
			if (msg == null) return;
			Console.WriteLine("\t[CH] Message not null!");

			int argPos = 0;
			if (!msg.HasStringPrefix("c!", ref argPos)) return;
			Console.WriteLine("\t[CH] Message has prefix!");

			Console.WriteLine("\tExecuting Command: " + msg + "!\n\n");
			var context = new SocketCommandContext(_client, msg);
			await _commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: _services);
		}
	}
}
