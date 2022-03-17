using CollectibleBot.Data;
using CollectibleBot.Data.Models;
using CollectibleBot.Modules.Utils;
using Discord.WebSocket;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Commands
{
	public class DropComponentModule : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
	{
		private readonly DbUtil _utils;
		private DropUtil _drop;

		public DropComponentModule(DbUtil utils, DropUtil drop)
		{
			Console.WriteLine("Started Drop Components");
			_utils = utils;
			_drop = drop;
		}

		[ComponentInteraction("drop-claim")]
		public async Task claim()
		{

			await Context.Interaction.DeferAsync();

			User user = _utils.getUserAsync(Context.User.Id.ToString(), Context.Guild.Id.ToString()).Result;

			if (_drop.claimItem(user).Result)
			{
				await ReplyAsync($"{Context.User.Username} claimed the {Item.rarityToString(_drop.drop.Rarity)} {_drop.drop.Name}!");
			}
			else
			{
				await RespondAsync("Uh-oh! You gotta be quicker than that! Drops only last 2 minutes!", ephemeral: true);
			}
		}
	}
}
