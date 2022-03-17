using CollectibleBot.Data;
using CollectibleBot.Data.Models;
using CollectibleBot.Modules.Utils;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Commands
{
	[Group("market", "Everything pertaining to the collectible market.")]
	public class MarketModule : InteractionModuleBase
	{
		private string GuildId;
		private string UserId;

		private readonly DbUtil _util;
		private readonly BotDb _ctx;
		private readonly InteractiveService _interact;

		public MarketModule(DbUtil util, BotDb ctx, InteractiveService interact)
		{
			_util = util;
			_ctx = ctx;
			_interact = interact;
		}

		[SlashCommand("list", "List all current market values")]
		public async Task List()
		{
			UserId = Context.User.Id.ToString();
			GuildId = Context.Guild.Id.ToString();
			EmbedBuilder embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.Guild.IconUrl,
					Name = Context.Guild.Name
				},
				Title = $"{Context.Guild.Name}'s Market",
				Description = "The Market holds everything pertaining to selling an item.\n" +
				"These prices are without any rarity bonuses and only serve as an estimate.\n" +
				"The prices of each item will fluctuate every so often as well.",

			};

			List<Collectible> items = _util.getCollectibles(GuildId);
			if (items == null || items.Count == 0 || _util.getMarkets(GuildId) == null || _util.getMarkets(GuildId).Count == 0)
			{
				Console.WriteLine("\tNo Collectibles Found");
				embed.AddField(new EmbedFieldBuilder
				{
					Name = "No collectibles found :(",
					Value = "Ask a server admin to add some collectibles!"
				});
				await RespondAsync(embed: embed.Build());
				return;
			}

			Console.WriteLine(_util.getMarkets(GuildId).Count);
			foreach (Market market in _util.getMarkets(GuildId))
			{
				Console.WriteLine($"{market.name}: {market.price}");
			}

			foreach (Collectible item in items)
			{
				Market market = await _util.getMarketValueAsync(item.Name, GuildId);
				embed.AddField(new EmbedFieldBuilder
				{
					Name = $"{market.name} | Current Price: ${market.price}",
					Value = $"Median Price: {item.Price}\n" +
					$"Flux percentage: %{item.Flux}",
					IsInline = true
				});
			}

			await RespondAsync(embed: embed.Build());
		}

		[SlashCommand("sell", "Sell any collectibles you have according to the market value!")]
		public async Task Sell(
			[Summary("Name", "The name of the item to sell")] string name,
			[Summary("Rarity", "Multiples of an item? Rule it down to the rarity.")]
			[Choice("Common", 1), Choice("Uncommon", 2), Choice("Rare", 3), Choice("Epic", 4), Choice("Legendary", 5)] int rarity = 0)
		{
			UserId = Context.User.Id.ToString();
			GuildId = Context.Guild.Id.ToString();

			await Context.Interaction.DeferAsync();
			await Context.Interaction.DeleteOriginalResponseAsync();

			Collectible c = _util.findItem(name, GuildId);
			if (c == null)
			{
				await RespondAsync("No item with that name exists! Make sure you're using proper capitalization!", ephemeral: true);
				return;
			}
			
			User user = await _util.getUserAsync(UserId, GuildId);
			Item item;
			if (rarity != 0) item = user.getItem(name, rarity);
			else item = user.getItem(name);

			if (item == null)
			{
				await RespondAsync("You don't seem to own that item! Make sure you're using proper capitalization!", ephemeral: true);
				return;
			}

			Market market = await _util.getMarketValueAsync(name, GuildId);
			AuctionHouse ah = await _util.getAHAsync(GuildId);

			double sold = (double)Decimal.Round((decimal)market.price * c.getRarityMult(item.Rarity), 2);

			var msg = await ReplyAsync(embed: EmbedUtil.MarketConfirmSell(Context, _util, item, sold).Build(), components: MarketUtil.ConfirmComponents().Build());
			var result = await _interact.NextMessageComponentAsync(x => x.Message.Id == msg.Id, timeout: TimeSpan.FromSeconds(120));

			if (result.IsSuccess) 
			{
				await result.Value!.DeferAsync();
			}

			await msg.DeleteAsync();

			if (result.Value.Data.CustomId == "no")
			{
				var cancel = await ReplyAsync("Alright! Cancelling the sell process.");
				await Task.Delay(2500);
				await cancel.DeleteAsync();
				return;
			}

			Console.WriteLine("Confirmed sell.");
			user.items.Remove(item);
			Console.WriteLine("Removed item.");
			user.coins += sold;
			Console.WriteLine("Added coins.");
			ah.listings.Add(new Item
			{
				Name = item.Name,
				Price = sold,
				Rarity = item.Rarity
			});
			Console.WriteLine("Listed!");

			await _ctx.SaveChangesAsync();
			Console.WriteLine("Saved");
			await ReplyAsync(embed: EmbedUtil.MarketSold(Context, _util, item, sold).Build());
		}
	}
}
