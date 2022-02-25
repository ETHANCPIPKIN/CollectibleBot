using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace maebot.Data.Models
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
        public int MaxRarity { get; set; }

        // The minimum rarity the collectible can be
        public int MinRarity { get; set; }

        // The median price. This is affected by the rarity
        // and fluctuation.
        // price + (price * (flux / 100) * (rarity + 1))
        public int Price { get; set; }

        // The max percentage price fluctuation.
        // minimum Flux is 0. Flux changes every hour.
        public int Flux { get; set; }

    }
}
