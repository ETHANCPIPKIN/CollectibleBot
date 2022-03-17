using CollectibleBot.Data;
using CollectibleBot.Data.Models;
using CollectibleBot.Modules.Utils;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Commands
{
	public class ItemComponentModule : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
	{
		private readonly BotDb _ctx;
		private readonly DbUtil _utils;
		private ItemUtil _item;
		private InteractiveService _interact;

		public ItemComponentModule(BotDb ctx, InteractiveService interact, DbUtil utils, ItemUtil item)
		{
			Console.WriteLine("Started Item Components", ConsoleColor.Blue);

			_ctx = ctx;
			_utils = utils;
			_item = item;
			_interact = interact;
		}

		[ComponentInteraction("item-setname")]
		public async Task createName()
		{
			Console.WriteLine("Running Set Name");

			var originalMessage = Context.Interaction;
			bool correct = false;

			await originalMessage.DeferAsync();

			while (!correct)
			{
				var m = await ReplyAsync("Type the name you wish to use:");
				SocketMessage response = null;

				response = _interact.NextMessageAsync().Result.Value;

				if (response == null) continue;

				Collectible collectible = _utils.findItem(response.Content, Context.Guild.Id.ToString());
				if (collectible != null)
				{
					var err = await ReplyAsync($"{response.Content} is already an item!");
					await Task.Delay(2500);
					await err.DeleteAsync();
				}
				else
				{
					_item.item.Name = response.Content;
					correct = true;
				}

				await response.DeleteAsync();
				await m.DeleteAsync();
			}

			await originalMessage.Message.ModifyAsync(x =>
			{
				x.Embed = _item.ItemEmbed(Context).Build();
				x.Components = _item.ItemComponents().Build();
			});
		}

		[ComponentInteraction("item-setprice")]
		public async Task createCost()
		{
			Console.WriteLine("Running Set Price");

			var originalMessage = Context.Interaction;
			bool correct = false;

			await originalMessage.DeferAsync();

			while (!correct)
			{
				var m = await ReplyAsync("Type the median price of the collectible (must be a whole number):");
				SocketMessage response = null;

				response = _interact.NextMessageAsync().Result.Value;

				if (response == null) continue;

				if (int.TryParse(response.Content, out int amt))
				{
					_item.item.Price = amt;
					correct = true;
				}
				else
				{
					var err = await ReplyAsync($"{response.Content} is not valid for an amount!");
					await Task.Delay(2500);
					await err.DeleteAsync();
				}

				await response.DeleteAsync();
				await m.DeleteAsync();
			}

			await originalMessage.Message.ModifyAsync(x =>
			{
				x.Embed = _item.ItemEmbed(Context).Build();
				x.Components = _item.ItemComponents().Build();
			});

			await DeferAsync();
		}

		[ComponentInteraction("item-setflux")]
		public async Task setLevel()
		{
			var originalMessage = Context.Interaction;

			var m = await ReplyAsync("Type the fluctuation percentage for this collectible, between 1 and 100");
			bool correct = false;
			SocketMessage response = null;

			await originalMessage.DeferAsync();

			while (!correct)
			{
				response = _interact.NextMessageAsync().Result.Value;

				if (response == null) continue;

				if (int.TryParse(response.Content, out int amt) && amt > 1 && amt < 100)
				{
					_item.item.Flux = amt;
					correct = true;
				}
				else
				{
					var err = await ReplyAsync($"{response.Content} is not valid for fluctuation!");
					await Task.Delay(2500);
					await err.DeleteAsync();
				}
			}

			await originalMessage.Message.ModifyAsync(x =>
			{
				x.Embed = _item.ItemEmbed(Context).Build();
				x.Components = _item.ItemComponents().Build();
			});

			await response.DeleteAsync();
			await m.DeleteAsync();

			await DeferAsync();
		}

		[ComponentInteraction("item-minrarity")]
		public async Task minRarity()
		{
			var original = Context.Interaction;
			bool correct = false;
			int rarity = 0;

			await original.DeferAsync();

			while (!correct)
			{
				var msg = await ReplyAsync("Set Minimum Rarity", components: _item.ItemMinRarityComponents().Build());

				// Wait for a user to press the button
				var result = await _interact.NextMessageComponentAsync(x => x.Message.Id == msg.Id, timeout: TimeSpan.FromSeconds(120));

				if (result.IsSuccess)
				{
					await result.Value!.DeferAsync();
				}

				rarity = int.Parse(result.Value.Data.CustomId);
				if (rarity > _item.item.MaxRarity && _item.item.MaxRarity != 0)
				{
					var err = await ReplyAsync($"You need to choose a lower rarity for your minimum!");
					await Task.Delay(2500);
					await err.DeleteAsync();
				}
				else
				{
					correct = true;
				}


				await msg.DeleteAsync();
			}

			_item.item.MinRarity = rarity;

			await original.Message.DeleteAsync();

			await ReplyAsync(embed: _item.ItemEmbed(Context).Build(), components: _item.ItemComponents().Build());
		}

		[ComponentInteraction("item-maxrarity")]
		public async Task maxRarity()
		{
			var original = Context.Interaction;
			bool correct = false;
			int rarity = 0;

			await original.DeferAsync();

			while (!correct)
			{
				var msg = await ReplyAsync("Set Maximum Rarity", components: _item.ItemMaxRarityComponents().Build());

				// Wait for a user to press the button
				var result = await _interact.NextMessageComponentAsync(x => x.Message.Id == msg.Id, timeout: TimeSpan.FromSeconds(120));

				if (result.IsSuccess)
				{
					await result.Value!.DeferAsync();
				}

				rarity = int.Parse(result.Value.Data.CustomId);
				if (rarity < _item.item.MinRarity)
				{
					var err = await ReplyAsync($"You need to choose a higher rarity for your maximum!!");
					await Task.Delay(2500);
					await err.DeleteAsync();
				}
				else
				{
					correct = true;
				}

				await msg.DeleteAsync();
			}

			_item.item.MaxRarity = rarity;

			await original.Message.DeleteAsync();

			await ReplyAsync(embed: _item.ItemEmbed(Context).Build(), components: _item.ItemComponents().Build());
		}

		[ComponentInteraction("ciend")]
		public async Task validateBonus()
		{
			Console.WriteLine("Running Validate Item!");

			Collectible item = _item.item;
			var original = Context.Interaction;

			if (item.Name == null || item.Flux == 0 || item.Price == 0 || item.MinRarity < 1 || item.MaxRarity < 1)
			{
				Console.WriteLine($"\tMissing some information!\n\t\t{item.Name} | ${item.Price}\n\t\t{item.MinRarity}, {item.MaxRarity}, %{item.Flux}");

				var m = await ReplyAsync("You're missing some informatin! Make sure you input everything!");

				await original.Message.ModifyAsync(x =>
				{
					x.Content = "";
					x.Embed = _item.ItemEmbed(Context).Build();
					x.Components = _item.ItemComponents().Build();
				});

				await Task.Delay(2500);
				await m.DeleteAsync();

				return;
			}

			Collectible test = _utils.findItem(item.Name, Context.Guild.Id.ToString());
			if (test != null && item.Name == test.Name && !_item.Edit)
			{
				Console.WriteLine($"The collectible name was taken! {item.Name}, {_item.Edit}");

				var m = await ReplyAsync("A collectible already exists with that name!");

				await original.Message.ModifyAsync(x =>
				{
					x.Content = "";
					x.Embed = _item.ItemEmbed(Context).Build();
					x.Components = _item.ItemComponents().Build();
				});

				await Task.Delay(2500);
				await m.DeleteAsync();

				return;
			}

			Console.WriteLine("\tPassed all checks...");

			if (!_item.Edit)
			{

				try
				{
					Console.WriteLine("\tAdding Item to Db...");
					await _ctx.Collectibles.AddAsync(item);
					await _ctx.Markets.AddAsync(new Market
					{
						name = item.Name,
						price = item.Price,
						guildId = Context.Guild.Id.ToString()
					});
					Console.WriteLine("\tAdded Item to Db!");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					await RespondAsync("There was an error saving your item!");
				}
			}

			try
			{
				Console.WriteLine("\tSaving Changes...");

				await _ctx.SaveChangesAsync();
				Console.WriteLine("Saved Changes!");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				await RespondAsync("There was an error saving your item!");
			}

			await original.Message.DeleteAsync();
			var complete = await ReplyAsync("Sucessfully created item!");
			await Task.Delay(5000);
			await complete.DeleteAsync();


			Console.WriteLine("Done!");
		}
	}
}
