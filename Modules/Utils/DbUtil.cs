using CollectibleBot.Data;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectibleBot.Data.Models;

namespace CollectibleBot.Modules.Utils
{
	public class DbUtil
	{
		readonly DiscordSocketClient _client;
		readonly BotDb _context;

		public DbUtil(DiscordSocketClient client, BotDb ctx)
		{
			_client = client;
			_context = ctx;
		}

		public async Task checkDb(string guildId, string userId)
		{
			Console.WriteLine("Checking Auctions...");
			await checkAuctions(guildId);
			Console.WriteLine("Checking User...");
			await checkUser(userId, guildId);
			Console.WriteLine("Checking Markets...");
			await checkMarkets(guildId);
			Console.WriteLine("Done!");
		}

		public async Task checkAuctions(string guildId)
		{
			var foundAH = await getAHAsync(guildId);
			if (foundAH == null)
			{
				Console.WriteLine("[DbUtil] (checkSales) No Auction House found, generating...");
				await _context.Auctions.AddAsync(new AuctionHouse
				{
					guildId = guildId,
					listings = new()
				});

				await _context.SaveChangesAsync();
				Console.WriteLine("[DbUtil] (checkSales) Auction House generated!");
			}

		}

		public async Task checkMarkets(string guildId)
		{
			List<Market> correctMarkets = getMarkets(guildId);

			if (correctMarkets == null)
			{
				Console.WriteLine("[DbUtil] (checkSales) No Market found, generating...");
				await generateMarkets(guildId);
				Console.WriteLine("[DbUtil] (checkSales) Market generated!");
			}
		}

		public async Task generateMarkets(string guildId)
		{
			List<Collectible> items = getCollectibles(guildId);
			if (items == null) return;

			foreach (Collectible item in items)
			{
				_context.Markets.Add(new Market
				{
					guildId = guildId,
					name = item.Name,
					price = item.generatePrice()
				});
			}

			await _context.SaveChangesAsync();
		}

		public async Task checkUser(string userId, string guildId)
		{
			var found = await getUserAsync(userId, guildId);
			if (found == null)
			{
				Console.WriteLine("[DbUtil] (checkUser) no User found, generating...");
				await _context.Users.AddAsync(new User
				{
					userId = userId,
					guildId = guildId,
					coins = 0,
					items = new List<Item>()
				});

				Console.WriteLine("[DbUtil] (checkUser) generated User!");
				await _context.SaveChangesAsync();
			}
		}

		public Collectible findItem(string name, string guildId)
		{
			try
			{
				List<Collectible> items = getCollectibles(guildId);
				if (items == null) return null;

				foreach(Collectible c in items)
				{
					if (c.Name.ToLower() == name.ToLower())
					{
						return c;
					}
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		public List<Collectible> findItems(int rarity, string guildId)
		{
			try
			{
				List<Collectible> items = getCollectibles(guildId);
				if (items == null) return null;

				List<Collectible> matching = new List<Collectible>();
				foreach (Collectible c in items)
				{
					if (c.MinRarity <= rarity && c.MaxRarity >= rarity)
					{
						matching.Add(c);
					}
				}

				return matching;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Market> getMarketValueAsync(string name, string guildId)
		{
			try
			{
				Market market = await _context.Markets.FirstOrDefaultAsync(market => market.name == name && market.guildId == guildId);
				return market;
			}
			catch
			{
				return null;
			}
		}

		public async Task<AuctionHouse> getAHAsync(string guildId)
		{
			try
			{
				AuctionHouse ah = await _context.Auctions.FirstOrDefaultAsync(ah => ah.guildId == guildId);
				return ah;
			}
			catch
			{
				return null;
			}
		}

		public async Task<User> getUserAsync(string userId, string guildId)
		{
			try
			{
				User user = await _context.Users.FirstOrDefaultAsync(user => user.userId == userId && user.guildId == guildId);
				return user;
			}
			catch
			{
				return null;
			}
		}

		public List<Market> getMarkets(string guildId)
		{
			try
			{
				List<Market> markets = _context.Markets.ToList().FindAll(m => m.guildId == guildId);
				if (markets.Count() < _context.Markets.ToList().Count())
				{
					return null;
				}
				return markets;
			}
			catch
			{
				return null;
			}
		}

		public List<Collectible> getCollectibles(string guildId)
		{
			try
			{
				List<Collectible> items = _context.Collectibles.ToList().FindAll(i => i.GuildId == guildId);
				if (items.Count < 1)
				{
					return null;
				}
				return items;
			}
			catch
			{
				return null;
			}
		}

		public List<User> getUsers(string guildId)
		{
			try
			{
				List<User> users = _context.Users.ToList().FindAll(u => u.guildId == guildId);
				if (users.Count < 1)
				{
					return null;
				}
				return users;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> removeItem(string name, string guildId)
		{
			Collectible item = findItem(name, guildId);
			if (item == null) return false;

			List<User> users = getUsers(guildId);
			foreach (User user in users)
			{
				if (user.items.Contains(item))
				{
					user.items.Remove(item);
				}
			}

			Market market = await getMarketValueAsync(name, guildId);
			if (market != null)
			{
				_context.Markets.Remove(market);
			}

			_context.Collectibles.Remove(item);
			await _context.SaveChangesAsync();

			return true;
		}
	}
}
