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
	// The Auction House holds all of the items that have been sold
	public class AuctionHouse
	{
		[Key]
		public int key { get; set; }

		public string guildId { get; set; }

		public List<Item> listings { get; set; }

		public Item getItem(string name)
		{
			foreach (Item item in listings)
			{
				if (item.Name == name) return item;
			}

			return null;
		}

		public Item getItem(string name, int rarity)
		{
			foreach (Item item in listings)
			{
				if (item.Name == name && item.Rarity == rarity) return item;
			}

			return null;
		}
	}
}
