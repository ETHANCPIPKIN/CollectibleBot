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
		public DbSet<Collectible> Collectibles { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string connection = EnvReader.GetStringValue("CONNECTION");
			Console.WriteLine(connection);

			optionsBuilder.UseSqlServer(connection);
		}
	}
}
