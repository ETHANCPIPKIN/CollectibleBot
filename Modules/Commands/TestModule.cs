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

		[SlashCommand("hello", "Hello World!")]
		public async Task Hello()
		{
			await ReplyAsync("Hello!");
		}

	}
}
