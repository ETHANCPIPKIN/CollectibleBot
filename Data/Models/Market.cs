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
	// This class represents the market value of an item.
	// This is what is referenced when a user sells an item.
	public class Market
	{
		[Key]
		public int id { get; set; }

		public string guildId { get; set; }

		public string name { get; set; }

		public double price { get; set; }
	}
}
