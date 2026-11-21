// <copyright file="WingCraftingOptions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Options of crafted second and third wings (and capes).
/// </summary>
internal static class WingCraftingOptions
{
    /// <summary>
    /// The weight of the health recovery option, when the type of the regular wing option is selected.
    /// Every other option (additional damage or defense) has twice the weight.
    /// </summary>
    private const int HealthRecoveryWeight = 1;

    private const int OtherOptionWeight = 2;

    /// <summary>
    /// Selects the type of the regular wing option; damage and defense are twice as likely as health recovery.
    /// </summary>
    /// <param name="options">The possible regular options of the wings.</param>
    /// <returns>The selected option.</returns>
    public static IncreasableItemOption? SelectRegularOption(IReadOnlyList<IncreasableItemOption> options)
    {
        return options.SelectWeightedRandom(options.Select(o =>
            o.PowerUpDefinition?.TargetAttribute == Stats.HealthRecoveryMultiplier ? HealthRecoveryWeight : OtherOptionWeight));
    }

    /// <summary>
    /// Adds one random wing option (e.g. ignore defense, +health) which the wings don't have yet.
    /// </summary>
    /// <param name="resultItem">The crafted wings.</param>
    /// <param name="player">The player.</param>
    public static void AddOneWingOption(Item resultItem, Player player)
    {
        var wingOption = resultItem.Definition!.PossibleItemOptions
            .SelectMany(o => o.PossibleOptions)
            .Where(o => o.OptionType == ItemOptionTypes.Wing && resultItem.ItemOptions.All(link => link.ItemOption != o))
            .SelectRandom();
        if (wingOption is null)
        {
            return;
        }

        var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
        link.ItemOption = wingOption;
        resultItem.ItemOptions.Add(link);
    }
}
