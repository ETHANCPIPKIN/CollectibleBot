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
			// Since AuctionHouse and User both use the ComplexType, Item,
			// it was important to tell the model builder that each type
			// could "own" Item.
			modelBuilder.Entity <AuctionHouse>(entity =>
			{
				entity.OwnsMany(e => e.listings);
			});
			modelBuilder.Entity<User>(entity =>
			{
				entity.OwnsMany(e => e.items);
			});
		}

		// Configures the DB to run with PostgreSQL
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string connection = EnvReader.GetStringValue("CONNECTION");

			optionsBuilder.UseNpgsql(connection);
		}
	}
}
