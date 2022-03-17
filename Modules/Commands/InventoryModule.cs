using CollectibleBot.Data.Models;
using CollectibleBot.Modules.Utils;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Commands
{
	public class InventoryModule : InteractionModuleBase
	{
		private readonly DbUtil _util;

		public InventoryModule(DbUtil util)
		{
			_util = util;
		}

		[SlashCommand("inventory", "Bring up your inventory")]
		public async Task inventory()
		{
			// Get user from DB
			User user = await _util.getUserAsync(Context.User.Id.ToString(), Context.Guild.Id.ToString());

			// Initialize the embed with basic info
			EmbedBuilder embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
					Name = Context.User.Username
				},
				Title = $"{Context.User.Username}'s Items \n${user.coins} | {user.items.Count} Items",
				Description = "Use ``/market sell`` to sell an item based on its current market value\n" +
				"Use ``/market buy`` to access the Auction House to see any available listings.",
				ThumbnailUrl = Context.User.GetAvatarUrl()
			};

			// Check if user has items
			if (user.items.Count < 1)
			{
				embed.AddField(new EmbedFieldBuilder
				{
					Name = "No items!",
					Value = "Chat for a bit until an item drops, or see if there is any on the Auction House!"
				});
				await RespondAsync(embed: embed.Build(), ephemeral: true);
				return;
			}
			
			// Go through the user's inventory and get all necessary data to display
			foreach (Item item in user.items)
			{
				Market market = await _util.getMarketValueAsync(item.Name, Context.Guild.Id.ToString());
				Collectible c = _util.findItem(item.Name, Context.Guild.Id.ToString());
				embed.AddField(new EmbedFieldBuilder
				{
					Name = $"{Item.rarityToString(item.Rarity)} {item.Name}",
					Value = $"Current Market Price: {market.price}\n" +
					$"Estimated Value: {item.Price * c.getRarityMult(item.Rarity)}"
				});
			}

			await RespondAsync(embed: embed.Build(), ephemeral: true);
		}
	}
}
