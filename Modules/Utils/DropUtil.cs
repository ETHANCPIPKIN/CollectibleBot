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

			dropTimer = new(2 * MINUTE);
			dropTimer.Elapsed += updateCollect;
			dropTimer.AutoReset = false;
		}

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

		// For use with dropping items the chances are:
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

		public ComponentBuilder claimComponents()
		{
			var component = new ComponentBuilder()
				.WithButton("Claim", "drop-claim");

			return component;
		}

		private void updateCollect(object source, ElapsedEventArgs e)
		{
			canCollect = false;
		}

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
