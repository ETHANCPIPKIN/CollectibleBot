using Discord;
using Discord.WebSocket;
using CollectibleBot.Data;
using CollectibleBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectibleBot.Modules.Utils
{
	// The ItemUtil holds a Collectible that gets edited via the item commands.
	// ItemUtil should be instantiated per guild, so no errors should occur cross-guild.
	public class ItemUtil
	{
		public Collectible item;

		// Edit is mostly referenced in ItemComponentModule as a means
		// to avoid duplicating items while editing
		public bool Edit = false;

		public ItemUtil()
		{
			item = new Collectible();
		}

		// This mainly checks if Edit is true.
		// If it is, track any changes made to item.
		public void setEditMode(bool mode, BotDb ctx)
		{
			Edit = mode;
			if (Edit)
			{
				ctx.Collectibles.Update(item);
			}
		}

		// Builds the basic item information embed for use in editing/creating items.
		public EmbedBuilder ItemEmbed(IInteractionContext Context)
		{
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.Guild.IconUrl,
					Name = Context.Guild.Name
				},
				Title = "Create Menu | Collectible",
				ThumbnailUrl = Context.Guild.IconUrl,
				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder
					{
						Name = item.Name ?? "No Name",
						Value = $"Median Price: ${item.Price}" ?? "Median Price: $0",
						IsInline = false
					},
					new EmbedFieldBuilder
					{
						Name = "Rarity and Fluxuation",
						Value = $"Min Rarity {item.rarityToString(item.MinRarity)}\n" +
						$"Max Rarity: {item.rarityToString(item.MaxRarity)}\n" +
						$"*Flux: %{item.Flux}*",
						IsInline = true
					},
				}
			};

			if (Edit)
			{
				embed.Title = "Edit Menu | Collectible";
			}

			return embed;
		}

		// Builds the components to create/edit an item
		public ComponentBuilder ItemComponents()
		{
			var component = new ComponentBuilder();

			if (!Edit)
			{
				component.WithButton("Set Name", "item-setname", row: 0);
			}

			component
				.WithButton("Set Median Price", "item-setprice", row: 0)
				.WithButton("Set Minimum Rarity", "item-minrarity", row: 1)
				.WithButton("Set Max Rarity", "item-maxrarity", row: 1)
				.WithButton("Set Flux", "item-setflux", row: 1)
				.WithButton("Finish Item", "ciend", row: 2);

			return component;
		}

		// Builds the components to select a minimum rarity, with logic
		// to avoid any errors.
		public ComponentBuilder ItemMinRarityComponents()
		{
			var component = new ComponentBuilder();

			component.WithButton("Common", "1");
			
			if (item.MaxRarity >= 2 || item.MaxRarity == 0) component
					.WithButton("Uncommon", "2");
			if (item.MaxRarity >= 3 || item.MaxRarity == 0) component
					.WithButton("Rare", "3");
			if (item.MaxRarity >= 4 || item.MaxRarity == 0) component
					.WithButton("Epic", "4");
			if (item.MaxRarity >= 5 || item.MaxRarity == 0) component
					.WithButton("Legendary", "5");

			return component;
		}

		// Builds the components to select a maximum rarity, with logic
		// to avoid any errors.
		public ComponentBuilder ItemMaxRarityComponents()
		{
			var component = new ComponentBuilder();

			if (item.MinRarity <= 1) component
					.WithButton("Common", "1");
			if (item.MinRarity <= 2) component
					.WithButton("Uncommon", "2");
			if (item.MinRarity <= 3) component
					.WithButton("Rare", "3");
			if (item.MinRarity <= 4) component
					.WithButton("Epic", "4");

			component.WithButton("Legendary", "5");

			return component;
		}
	}
}
