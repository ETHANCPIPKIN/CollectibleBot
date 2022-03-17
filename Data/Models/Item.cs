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
	// The item generated from Collectible, and what is stored in the
	// Auction House and User Inventory
	[ComplexType]
	public class Item
	{
		public string Name { get; set; }

		public int Rarity { get; set; }

		public double Price { get; set; }

        public static string rarityToString(int rarity)
        {
            switch (rarity)
            {
                case 1:
                    return "Common";
                case 2:
                    return "Uncommon";
                case 3:
                    return "Rare";
                case 4:
                    return "Epic";
                case 5:
                    return "Legendary";
            }

            return "Unknown";
        }
    }
}
