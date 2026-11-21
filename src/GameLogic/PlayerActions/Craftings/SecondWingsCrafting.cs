// <copyright file="SecondWingsCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;

/// <summary>
/// Crafting for Second Wings and the capes of Dark Lord and Rage Fighter.
/// Luck and the level of the item option are inherited from the mixed equipment; the wings always get one wing option.
/// </summary>
public class SecondWingsCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecondWingsCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public SecondWingsCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
    }

    /// <inheritdoc/>
    protected override void AddRandomItemOption(Item resultItem, Player player, byte successRate, MixInput input)
    {
        AddInheritedItemOption(resultItem, player, input, WingCraftingOptions.SelectRegularOption);
    }

    /// <inheritdoc/>
    protected override void AddRandomExcellentOptions(Item resultItem, Player player)
    {
        WingCraftingOptions.AddOneWingOption(resultItem, player);
    }
}
