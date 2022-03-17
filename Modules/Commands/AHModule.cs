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
	[Group("auctions", "Everything pertaining to buying collectibles.")]
	public class AHModule : InteractionModuleBase
	{
		private string GuildId;
		private string UserId;

		private readonly DbUtil _util;
		private readonly BotDb _ctx;
		private readonly InteractiveService _interact;

		public AHModule(DbUtil util, BotDb ctx, InteractiveService interact)
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
				Title = $"{Context.Guild.Name}'s Auction House",
				Description = "This is the Auction House, every item within the server that is sold lays here.\n" +
				"The price of the item never fluctuates in the House, so it's a great way to hone in on exactly what you want."

			};

			AuctionHouse ah = await _util.getAHAsync(GuildId);
			if (ah.listings.Count < 1)
			{
				Console.WriteLine("\tNothing is listed in the House");
				embed.AddField(new EmbedFieldBuilder
				{
					Name = "No listings found :(",
					Value = "Wait for someone to sell an item!"
				});
				await RespondAsync(embed: embed.Build());
				return;
			}

			foreach (Item item in ah.listings)
			{
				Collectible c = _util.findItem(item.Name, GuildId);
				embed.AddField(EmbedUtil.MarketItemPreview(c, item, item.Price));
			}

			await RespondAsync(embed: embed.Build());
		}

		[SlashCommand("buy", "Buy any collectibles available on the Auction House")]
		public async Task buy(
			[Summary("Name", "The name of the item to sell")] string name,
			[Summary("Rarity", "Multiples of an item? Rule it down to the rarity.")]
			[Choice("Common", 1), Choice("Uncommon", 2), Choice("Rare", 3), Choice("Epic", 4), Choice("Legendary", 5)] int rarity = 0)
		{
			GuildId = Context.Guild.Id.ToString();
			UserId = Context.User.Id.ToString();

			await Context.Interaction.DeferAsync();
			await Context.Interaction.DeleteOriginalResponseAsync();

			if (_util.findItem(name, GuildId) == null)
			{
				await RespondAsync("No item with that name exists! Make sure you're using proper capitalization!", ephemeral: true);
				return;
			}

			AuctionHouse ah = await _util.getAHAsync(GuildId);
			if (ah.listings.Count < 1)
			{
				await RespondAsync("There's nothing in the Auction House yet! Wait for someone to sell an item or wait for an item drop!");
				return;
			}

			Item item;
			if (rarity != 0) item = ah.getItem(name, rarity);
			else item = ah.getItem(name);

			if (item == null)
			{
				await RespondAsync("That item isn't in the Auction House yet! Wait for someone to sell an item or wait for an item drop!", ephemeral: true);
				return;
			}

			User user = await _util.getUserAsync(UserId, GuildId);
			if (user.coins < item.Price)
			{
				await RespondAsync("You don't have enough coins yet to purchase this! Get some more collectibles to sell!");
				return;
			}

			double sold = (double) Decimal.Round((decimal) (item.Price + (item.Price * 0.05f)), 2);
			var msg = await ReplyAsync(embed: EmbedUtil.MarketConfirmPurchase(Context, _util, item, user, sold).Build(), components: MarketUtil.ConfirmComponents().Build());
			var result = await _interact.NextMessageComponentAsync(x => x.Message.Id == msg.Id, timeout: TimeSpan.FromSeconds(120));

			if (result.IsSuccess) await result.Value!.DeferAsync();

			await msg.DeleteAsync();

			if (result.Value.Data.CustomId == "no")
			{
				var cancel = await ReplyAsync("Alright! Cancelling the purchase.");
				await Task.Delay(2500);
				await cancel.DeleteAsync();
				return;
			}

			ah.listings.Remove(item);
			user.coins -= sold;
			user.items.Add(item);

			await _ctx.SaveChangesAsync();
			await ReplyAsync(embed: EmbedUtil.MarketBought(Context, _util, item, sold).Build());
		}
	}
}
