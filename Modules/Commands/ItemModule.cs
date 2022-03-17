using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using CollectibleBot.Data;
using CollectibleBot.Data.Models;
using CollectibleBot.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Commands
{
	[Group("items", "All commands that interact with collectibles")]
	public class ItemModule : InteractionModuleBase
	{
		private readonly BotDb _ctx;
		private readonly DbUtil _utils;
		private ItemUtil _item;

		public ItemModule(BotDb ctx, DbUtil utils, ItemUtil item)
		{
			Console.WriteLine("Started Item Module", ConsoleColor.Blue);

			_ctx = ctx;
			_utils = utils;
			_item = item;
		}

		[SlashCommand("info", "Get all item info from this guild")]
		public async Task Info()
		{
			Console.WriteLine("\tRunning command...");

			EmbedBuilder embed = EmbedUtil.ItemInfo(Context, _utils);

			if (embed == null)
			{
				await RespondAsync("No items found!", ephemeral: true);
				return;
			}

			await RespondAsync(embed: embed.Build());
		}

		[SlashCommand("create", "Bring up the dialogue to create a new item")]
		public async Task Create()
		{
			// Instantiate a new Collectible and autofill the GuildId
			_item.item = new Collectible
			{
				GuildId = Context.Guild.Id.ToString()
			};

			await RespondAsync(embed: _item.ItemEmbed(Context).Build(), components: _item.ItemComponents().Build());
		}

		[SlashCommand("edit", "Bring up the dialogue to edit an item")]
		public async Task Edit(
			[Summary("Name", "The name of the item to edit")] string name)
		{

			// Check if the collectible exists
			Collectible item = _utils.findItem(name, Context.Guild.Id.ToString());
			if (item == null)
			{
				await RespondAsync($"No item with the name \"{name}\" was found!", ephemeral: true);
				return;
			}

			// If the collectible exists, set the current item to it and put ItemUtil into edit mode
			_item.item = item;
			_item.setEditMode(true, _ctx);

			await RespondAsync(embed: _item.ItemEmbed(Context).Build(), components: _item.ItemComponents().Build());
		}

		[SlashCommand("delete", "Delete an item")]
		public async Task Delete(
			[Summary("Name", "The name of the item to delete")] string name)
		{
			bool result = await _utils.removeItem(name, Context.Guild.Id.ToString());

			if (result)
			{
				await RespondAsync($"Item {name} deleted from database!", ephemeral: true);
			}
			else await RespondAsync($"No item with the name \"{name}\" was found!", ephemeral: true);

		}
	}
}
