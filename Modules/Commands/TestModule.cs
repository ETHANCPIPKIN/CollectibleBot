using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Commands
{
	public class TestModule : InteractionModuleBase
	{
		public TestModule()
		{
			Console.WriteLine("Started Test Module");
		}

		// Simply a test to see if the bot works
		[SlashCommand("hello", "Hello World!")]
		public async Task Hello()
		{
			await RespondAsync("Hello!");
		}

	}
}
