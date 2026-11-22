// <copyright file="ChaosWeaponAndFirstWingsCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

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
    /// <remarks>
    /// A chaos weapon in the machine selects the mix by the jewels besides it: without Bless and Soul it's the first
    /// wings, with them it's a level upgrade (+10 and higher). So a chaos weapon isn't the material for another
    /// chaos weapon, and the wings don't take Bless or Soul.
    /// </remarks>
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        if (base.TryGetRequiredItems(player, out items, out successRate) is { } error)
        {
            return error;
        }

        var isWingsCrafting = items.Any(link => link.ItemRequirement.PossibleItems.Any()
                                                && link.ItemRequirement.PossibleItems.All(IsChaosWeapon));
        if (isWingsCrafting)
        {
            if (items.Any(link => link.Items.Any(i => i.Definition is { Group: 14, Number: 13 or 14 })))
            {
                return CraftingResult.IncorrectMixItems;
            }
        }
        else if (items.Any(link => !link.ItemRequirement.PossibleItems.Any() && link.Items.Any(i => i.Definition is { } d && IsChaosWeapon(d))))
        {
            return CraftingResult.IncorrectMixItems;
        }

        return null;
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

    /// <summary>Chaos Dragon Axe, Chaos Nature Bow and Chaos Lightning Staff.</summary>
    private static bool IsChaosWeapon(ItemDefinition definition)
        => definition is { Group: 2, Number: 6 } or { Group: 4, Number: 6 } or { Group: 5, Number: 7 };
}
