using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Utils
{
	public class MarketUtil
	{

		public static ComponentBuilder ConfirmComponents()
		{
			var component = new ComponentBuilder()
				.WithButton("Yes", "yes")
				.WithButton("No", "no");

			return component;
		}

	}
}
