using CollectibleBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Discord;
using CollectibleBot.Data;

namespace CollectibleBot.Modules.Utils
{
	// The DropUtil holds an Item that is generated whenever a Drop happens in the Program
	// Any user can claim the item as long as it is collected within one minute, otherwise
	// the item is marked as unclaimable (canCollect = false)
	public class DropUtil
	{
		public Item drop;
		public Timer dropTimer;
		public bool canCollect = false;

		const int MINUTE = 60000;
		private BotDb _ctx;

		public DropUtil(BotDb ctx)
		{
			_ctx = ctx;
			drop = null;

			dropTimer = new(MINUTE);
			dropTimer.Elapsed += updateCollect;
			dropTimer.AutoReset = false;
		}

		// Generates a random item to hold as a drop
		public void generateItem(Collectible c)
		{
			int rarity = rollRarity();
			Item item = new Item
			{
				Name = c.Name,
				Price = c.Price,
				Rarity = rarity
			};

			canCollect = true;
			drop = item;
		}

		// For use with dropping items.
		// The chances are:
		// 50% Common, 20% Uncommon, 15% Rare, 10% Epic, 5% Legendary
		public int rollRarity()
		{
			Random rand = new();
			int chance = rand.Next(1, 100);

			if (chance < 50) return 1;
			else if (chance < 70) return 2;
			else if (chance < 85) return 3;
			else if (chance < 95) return 4;
			else return 5;

		}

		// Builds the embed to display information about the drop
		public EmbedBuilder dropEmbed(IGuild guild)
		{
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = guild.IconUrl,
					Name = guild.Name
				},
				Title = "New Collectible Drop!",
				Description = $"{Item.rarityToString(drop.Rarity)} {drop.Name}\n" +
				$"**Median Price:** {drop.Price}"
			};

			return embed;
		}

		// Builds the Claim button to attach to the embed
		public ComponentBuilder claimComponents()
		{
			var component = new ComponentBuilder()
				.WithButton("Claim", "drop-claim");

			return component;
		}

		// This is for when the Timer elapses, to make the item unclaimable
		private void updateCollect(object source, ElapsedEventArgs e)
		{
			canCollect = false;
		}

		// Checks if the item can be claimed, if it can, update the User's inventory accordingly
		public async Task<bool> claimItem(User user)
		{
			if (!canCollect || drop == null) return false;

			_ctx.Update(user);
			user.items.Add(drop);
			await _ctx.SaveChangesAsync();

			return true;
		}
	}
}
