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
	public class Collectible
	{

        [Key]
        public int id { get; set; }

        // The name of the collectible
        public string Name { get; set; }

        // The guild id the collectible is set in
        public string GuildId { get; set; }

        // The max rarity the collectible can be
        // 1 common, 2 uncommon, 3 rare, 4 epic, 5 legendary
        public int MaxRarity { get; set; } = 0;

        // The minimum rarity the collectible can be
        public int MinRarity { get; set; } = 0;

        // The median price. This is affected by the rarity
        // and fluctuation.
        // price + (price * (flux / 100) * (rarity + 1))
        public int Price { get; set; }

        // The max percentage price fluctuation.
        // The minimum flucutation is always -25
        // just to keep the price positive always.
        public int Flux { get; set; }

        // For use within code to see the effect of rarities
        public int generateRarity()
		{
            Random rand = new();
            return rand.Next(MinRarity, MaxRarity);
		}

        // Generate price can work in 2 seperate ways:
        // 1. To get a market price adjustment with no rarity
        // 2. To generate an item for a user to claim.
        public double generatePrice(int rarity = 0)
		{
            Random rand = new();

            double newFlux = (double) rand.Next(-25, Flux) / 100f;
            Console.WriteLine($"Generated Flux: {newFlux}");
            double newPrice = Price + (Price * newFlux);
            Console.WriteLine($"Generated Price: {newPrice}");
            if (rarity != 0)
			{
                newPrice *= getRarityMult(rarity);
			}

            return newPrice;
		}

        // The rarity multiplier is applied at the point of sale to the market value.
        // For added randomness, Flux plays a role in generating the multiplier.
        // The lower the Flux, the worse potential multiplier
        public int getRarityMult(int rarity)
        {
            Random rand = new();
            int flux = rand.Next(Flux, 100);
            int bonus = rarity + (rarity * (flux / 100));

            return bonus;
        }

        // Takes a rarity id and returns the correct string
        public string rarityToString(int rarity)
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

        // Sometimes it was essential to convert a Collectible to an Item when comparing market values or testing rarities.
        public static implicit operator Item(Collectible c) => new Item
        {
            Name = c.Name,
            Rarity = c.generateRarity(),
            Price = c.Price
        };

    }
}
