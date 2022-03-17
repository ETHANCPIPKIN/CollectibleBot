using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Data.Models
{
	public class User
	{
		[Key]
		public int id { get; set; }

		public string userId { get; set; }

		public string guildId { get; set; }

		public double coins { get; set; }

		public List<Item> items { get; set; }

		public Item getItem(string name)
		{
			foreach (Item item in items)
			{
				if (item.Name == name) return item;
			}

			return null;
		}

		public Item getItem(string name, int rarity)
		{
			foreach (Item item in items)
			{
				if (item.Name == name && item.Rarity == rarity) return item;
			}

			return null;
		}
	}
}
