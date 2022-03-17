using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Utils
{
	// More features were planned for MarketUtil, but
	// unfortunately ran out of time to implement them.
	public class MarketUtil
	{
		// Builds the components to confirm an action
		public static ComponentBuilder ConfirmComponents()
		{
			var component = new ComponentBuilder()
				.WithButton("Yes", "yes")
				.WithButton("No", "no");

			return component;
		}

	}
}
