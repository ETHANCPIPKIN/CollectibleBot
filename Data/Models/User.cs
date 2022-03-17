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
	// The user is generated from a user's message. 
	public class User
	{
		[Key]
		public int id { get; set; }

		// The Discord Snowflake ID of the user
		public string userId { get; set; }

		// The Discord Snowflake ID of the guild
		public string guildId { get; set; }
		
		// The amount of currency the user holds
		public double coins { get; set; }

		// Every item that the user has
		public List<Item> items { get; set; }

		// Gets an item from the list by name
		public Item getItem(string name)
		{
			foreach (Item item in items)
			{
				if (item.Name == name) return item;
			}

			return null;
		}

		// Gets an item from the list by name and rarity
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
