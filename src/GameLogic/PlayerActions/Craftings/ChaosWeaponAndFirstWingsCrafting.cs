// <copyright file="ChaosWeaponAndFirstWingsCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;

/// <summary>
/// Crafting for Chaos Weapon and First Wings.
/// Luck and the item option are inherited from the mixed equipment, the skill is always added.
/// </summary>
public class ChaosWeaponAndFirstWingsCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChaosWeaponAndFirstWingsCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public ChaosWeaponAndFirstWingsCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
    }

    /// <inheritdoc/>
    protected override ItemCraftingResultItem SelectResultItem(ICollection<ItemCraftingResultItem> results, IList<CraftingRequiredItemLink> requiredItems, Player player)
    {
        return CraftingResultClassSelection.Select(results, requiredItems, player);
    }

    /// <inheritdoc/>
    protected override void AddRandomItemOption(Item resultItem, Player player, byte successRate, MixInput input)
    {
        if (input.HasEquipment)
        {
            AddInheritedItemOption(resultItem, player, input, options => options[0]);
            return;
        }

        if (resultItem.Definition!.PossibleItemOptions.FirstOrDefault(o =>
                    o.PossibleOptions.Any(p => p.OptionType == ItemOptionTypes.Option))
                is { } option)
        {
            int i = Rand.NextInt(0, 3);
            if (Rand.NextRandomBool((successRate / 5) + (4 * (i + 1))))
            {
                var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
                link.ItemOption = option.PossibleOptions.First();
                link.Level = 3 - i;
                resultItem.ItemOptions.Add(link);
            }
        }
    }

    /// <inheritdoc/>
    protected override void AddRandomLuckOption(Item resultItem, Player player, byte successRate, MixInput input)
    {
        if (input.HasEquipment)
        {
            base.AddRandomLuckOption(resultItem, player, successRate, input);
            return;
        }

        if (Rand.NextRandomBool((successRate / 5) + 4)
            && resultItem.Definition!.PossibleItemOptions.FirstOrDefault(o =>
                    o.PossibleOptions.Any(po => po.OptionType == ItemOptionTypes.Luck))
                is { } luck)
        {
            var luckOption = player.PersistenceContext.CreateNew<ItemOptionLink>();
            luckOption.ItemOption = luck.PossibleOptions.First();
            resultItem.ItemOptions.Add(luckOption);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// On this server a chaos weapon always gets its skill.
    /// </remarks>
    protected override void AddRandomSkill(Item resultItem, byte successRate, MixInput input)
    {
        if (resultItem.Definition!.Skill is { })
        {
            resultItem.HasSkill = true;
        }
    }
}
