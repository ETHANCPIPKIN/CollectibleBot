using Discord;
using Discord.WebSocket;
using CollectibleBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Utils
{
	class EmbedUtil
	{
		// Builds the embed to list all of the items and their basic information
		public static EmbedBuilder ItemInfo(IInteractionContext Context, DbUtil util)
		{
			EmbedBuilder embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.Guild.IconUrl,
					Name = Context.Guild.Name
				},
				Title = $"{Context.Guild.Name}'s Collectibles",
				Description = "These collectibles drop randomly within the server, " +
				"You can buy collectibles from someone else in the **Auction House**.\n" +
				"The Auction House can be accessed by using ``/market ah``."
			};

			List<Collectible> items = util.getCollectibles(Context.Guild.Id.ToString());
			if (items == null || items.Count == 0)
			{
				Console.WriteLine("\tNo Collectibles Found");
				embed.AddField(new EmbedFieldBuilder
				{
					Name = "No collectibles found :(",
					Value = "Ask a server admin to add some collectibles!"
				});
				return embed;
			}

			foreach (Collectible item in items)
			{
				embed.AddField(new EmbedFieldBuilder
				{
					Name = $"**{item.Name}** | Median Price: ${item.Price}",
					Value = $"Min Rarity: {item.rarityToString(item.MinRarity)}\n" +
					$"Max Rarity: {item.rarityToString(item.MaxRarity)}\n" +
					$"*Flux: %{item.Flux}*",
					IsInline = true
				});
			}

			return embed;

		}

		// Common code to build a field for item information
		public static EmbedFieldBuilder MarketItemPreview(Collectible c, Item item, double price)
		{
			EmbedFieldBuilder field = new EmbedFieldBuilder
			{
				Name = $"{Item.rarityToString(item.Rarity)} {item.Name} | ${price}",
				Value = $"Median Price: {c.Price}\n" +
				$"Flux Percentage: %{c.Flux}"
			};

			return field;
		}

		// Builds an embed to notify that an item was sold to the Auction House
		public static EmbedBuilder MarketSold(IInteractionContext Context, DbUtil _util, Item item, double price)
		{
			EmbedBuilder embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.User.GetAvatarUrl(),
					Name = Context.User.Username
				},
				Title = "AAAAND SOLD!!!",
				Description = $"{Context.User.Username} just sold this item to the Auction House for ${price}!!!"
			};

			Collectible c = _util.findItem(item.Name, Context.Guild.Id.ToString());

			embed.AddField(MarketItemPreview(c, item, price));

			return embed;
		}

		// Builds an embed to ask if the user really wants to sell their item.
		// Displays all necessary information pertaining to selling an item
		public static EmbedBuilder MarketConfirmSell(IInteractionContext Context, DbUtil _util, Item item, double price)
		{
			EmbedBuilder embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.Guild.IconUrl,
					Name = Context.Guild.Name
				},
				Title = $"Selling {item.Name}",
				Description = $"Are you sure you want to sell this item for ${price}?"
			};

			Collectible c = _util.findItem(item.Name, Context.Guild.Id.ToString());
			embed.AddField(MarketItemPreview(c, item, price));

			return embed;

		}

		// Builds an embed to notify that an item was Bought from the Auction House
		public static EmbedBuilder MarketBought(IInteractionContext Context, DbUtil _util, Item item, double price)
		{
			EmbedBuilder embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.User.GetAvatarUrl(),
					Name = Context.User.Username
				},
				Title = "AAAAND SOLD!!!",
				Description = $"{Context.User.Username} just bought this awesome item from the Auction House for ${price}!!!"
			};

			Collectible c = _util.findItem(item.Name, Context.Guild.Id.ToString());

			embed.AddField(MarketItemPreview(c, item, item.Price));

			return embed;
		}

		// Builds an embed to ask if the user really wants to buy this item.
		// Displays all necessary information pertaining to buying an item.
		public static EmbedBuilder MarketConfirmPurchase(IInteractionContext Context, DbUtil _util, Item item, User user, double price)
		{
			EmbedBuilder embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.Guild.IconUrl,
					Name = Context.Guild.Name
				},
				Title = $"Buying {item.Name}",
				Description = $"Are you sure you want to buy this item for {price}?\n" +
				$"*Current Balance: ${user.coins}*\n" +
				$"*Expected Balance: ${user.coins - price}*"
			};

			Collectible c = _util.findItem(item.Name, Context.Guild.Id.ToString());

			embed.AddField(MarketItemPreview(c, item, item.Price));

			return embed;
		}
	}
}
