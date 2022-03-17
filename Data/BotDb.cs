using dotenv.net;
using dotenv.net.Utilities;
using CollectibleBot.Data.Models;
using CollectibleBot.Modules;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Data
{
	public class BotDb : DbContext
	{
		public DbSet<AuctionHouse> Auctions { get; set; }

		public DbSet<Collectible> Collectibles { get; set; }
		public DbSet<Market> Markets { get; set; }
		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity <AuctionHouse>(entity =>
			{
				entity.OwnsMany(e => e.listings);
			});
			modelBuilder.Entity<User>(entity =>
			{
				entity.OwnsMany(e => e.items);
			});
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string connection = EnvReader.GetStringValue("CONNECTION");
			Console.WriteLine(connection);

			optionsBuilder.UseNpgsql(connection);
		}
	}
}
