using Discord;
using Discord.Commands;
using Discord.Interactions;
using Fergun.Interactive;
using Discord.WebSocket;
using CollectibleBot.Data;
using CollectibleBot.Data.Models;
using CollectibleBot.Modules.Utils;
using CollectibleBot.Modules.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IResult = Discord.Interactions.IResult;

namespace CollectibleBot.Modules
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly InteractionService _interact;
		private IServiceProvider _services;

		public CommandHandler(IServiceProvider services)
		{
			_services = services;

			_client = services.GetService<DiscordSocketClient>();
			_interact = services.GetService<InteractionService>();
		}

		public async Task InstallCommandsAsync()
		{
			// Add Action Events for whenever the user interacts with the bot
			_client.SlashCommandExecuted += HandleInteractionCommandAsync;
			_client.SelectMenuExecuted += HandleComponentInteractionAsync;
			_client.ButtonExecuted += HandleComponentInteractionAsync;

			// Load all of the Modules into the interact service.
			// This just takes all of the public modules that inherit
			// some version of InterationModuleBase.
			await _interact.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			// Register commands to test guild for quicker access
			await _interact.RegisterCommandsToGuildAsync(951307889857933353);

			// Register commands globally.
			// This takes about 30 minutes to an hour to take full effect,
			// just due to Discord's functions.
			await _interact.RegisterCommandsGloballyAsync();

			// Add Logging Events to display information about what is happening within interactions
			_interact.SlashCommandExecuted += SlashCommandExecuted;
			_interact.ContextCommandExecuted += ContextCommandExecuted;
			_interact.ComponentCommandExecuted += ComponentCommandExecuted;

			Console.WriteLine("[STARTUP] Installed Commands!");
		}

		/*
		 * This method handles Text commands. This is not necessary as the bot is only using the Discord Interaction API
		private async Task HandleCommandAsync(SocketMessage messageParam)
		{
			if (messageParam.Channel is SocketDMChannel || messageParam.Author.IsBot) return;
			Console.WriteLine("Recieved Message!");

			var message = messageParam as SocketUserMessage;
			if (message == null) return;
			Console.WriteLine("\tMessage not null!");

			int argPos = 0;
			if (!message.HasStringPrefix("cb!", ref argPos)) return;
			Console.WriteLine("\t\tMessage has prefix!");

			Console.WriteLine("\tExecuting command: " + message + "!");
			var context = new SocketCommandContext(_client, message);
			await _commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: _services);
		}
		*/

		// This tries to execute a command from a Component (i.e: Button/Menu)
		private async Task HandleComponentInteractionAsync(SocketMessageComponent interact)
		{
			try
			{
				Console.WriteLine($"\tExecuting command: {interact}! " +
					$"\n\tId: {interact.Data.CustomId}\n\tValues: {interact.Data.Values}");
				var context = new SocketInteractionContext<SocketMessageComponent>(_client, interact);
				await _interact.ExecuteCommandAsync(
					context: context,
					services: _services);
				Console.WriteLine($"\tDone!");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);

				if (interact.Type == InteractionType.ApplicationCommand)
				{
					await interact.GetOriginalResponseAsync()
						.ContinueWith(async (msg) => await msg.Result.DeleteAsync());
				}
			}
		}

		// This tries to execute a command from a User (i.e: Slash Command)
		private async Task HandleInteractionCommandAsync(SocketInteraction interact)
		{
			try
			{
				Console.WriteLine("\tExecuting command: " + interact + "!");
				var context = new SocketInteractionContext(_client, interact);
				await _interact.ExecuteCommandAsync(
					context: context,
					services: _services);
				Console.WriteLine($"\tDone!");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);

				if (interact.Type == InteractionType.ApplicationCommand)
				{
					await interact.GetOriginalResponseAsync()
						.ContinueWith(async (msg) => await msg.Result.DeleteAsync());
				}
			}
		}

		private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
		{
			if (!arg3.IsSuccess)
			{
				switch (arg3.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						Console.WriteLine($"[ComponentEx] Error: UnmetPrecondition");
						break;
					case InteractionCommandError.UnknownCommand:
						Console.WriteLine($"[ComponentEx] Error: UnknownCommand");
						break;
					case InteractionCommandError.BadArgs:
						Console.WriteLine($"[ComponentEx] Error: BadArgs");
						break;
					case InteractionCommandError.Exception:
						Console.WriteLine($"[ComponentEx] Error: UnknownException");
						Console.WriteLine($"\n\tCommand Info: {arg1}\n\tInteraction Context: {arg2.Interaction}\n\tResult: {arg3}\n\t\t{arg3.Error}");
						break;
					case InteractionCommandError.Unsuccessful:
						Console.WriteLine($"[ComponentEx] Error: Unsuccessful");
						break;
					default:
						break;
				}
			}

			return Task.CompletedTask;
		}

		private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
		{
			if (!arg3.IsSuccess)
			{
				switch (arg3.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						Console.WriteLine($"[ContextEx] Error: UnmetPrecondition");
						break;
					case InteractionCommandError.UnknownCommand:
						Console.WriteLine($"[ContextEx] Error: UnknownCommand");
						break;
					case InteractionCommandError.BadArgs:
						Console.WriteLine($"[ContextEx] Error: BadArgs");
						break;
					case InteractionCommandError.Exception:
						Console.WriteLine($"[ContextEx] Error: UnknownException");
						break;
					case InteractionCommandError.Unsuccessful:
						Console.WriteLine($"[ContextEx] Error: Unsuccessful");
						break;
					default:
						break;
				}
			}

			return Task.CompletedTask;
		}

		private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
		{
			if (!arg3.IsSuccess)
			{
				switch (arg3.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						Console.WriteLine($"[SlashEx] Error: UnmetPrecondition");
						break;
					case InteractionCommandError.UnknownCommand:
						Console.WriteLine($"[SlashEx] Error: UnknownCommand");
						break;
					case InteractionCommandError.BadArgs:
						Console.WriteLine($"[SlashEx] Error: BadArgs");
						break;
					case InteractionCommandError.Exception:
						Console.WriteLine($"[SlashEx] Error: UnknownException");
						break;
					case InteractionCommandError.Unsuccessful:
						Console.WriteLine($"[SlashEx] Error: Unsuccessful");
						break;
					default:
						break;
				}
			}
			else
			{
				Console.WriteLine($"[SlashEx] Command Executed Successfully");
			}

			return Task.CompletedTask;
		}

	}

}

