// <copyright file="DinorantCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// Crafting for Dinorant.
/// </summary>
public class DinorantCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DinorantCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public DinorantCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
    }

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        var craftingResult = base.TryGetRequiredItems(player, out items, out successRate);
        if (craftingResult is null)
        {
            var uniriaLink = items.Where(i => i.ItemRequirement.PossibleItems.Any(i => new ItemIdentifier(i.Number, i.Group) == ItemConstants.Uniria));
            foreach (var item in uniriaLink.First().Items)
            {
                if (item.Durability < 255)
                {
                    return CraftingResult.IncorrectMixItems;
                }
            }
        }

        return craftingResult;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// On this server the dinorant always gets all of its options.
    /// </remarks>
    protected override void AddRandomItemOption(Item resultItem, Player player, byte successRate, MixInput input)
    {
        if (resultItem.Definition!.PossibleItemOptions.FirstOrDefault(o =>
                    o.PossibleOptions.Any(p => p.OptionType == ItemOptionTypes.Option))
                is { } options)
        {
            foreach (var option in options.PossibleOptions)
            {
                var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
                link.ItemOption = option;
                resultItem.ItemOptions.Add(link);
            }
        }
    }
}
