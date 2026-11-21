// <copyright file="MixInput.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Items;

using MUnique.OpenMU.DataModel.Configuration.Items;

/// <summary>
/// The options of the equipment items (items which can have luck) which were put into a mix.
/// The result item inherits them: the more input items have an option, the more likely the result gets it.
/// </summary>
/// <remarks>
/// The options are copied, because the input items get destroyed before the result item is created.
/// </remarks>
public sealed class MixInput
{
    private readonly IReadOnlyList<EquipmentOptions> _equipment;

    private MixInput(IReadOnlyList<EquipmentOptions> equipment)
    {
        this._equipment = equipment;
    }

    /// <summary>
    /// Gets an input without equipment.
    /// </summary>
    public static MixInput Empty { get; } = new([]);

    /// <summary>
    /// Gets a value indicating whether equipment was put into the mix.
    /// </summary>
    public bool HasEquipment => this._equipment.Count > 0;

    /// <summary>
    /// Creates the input of the specified items. Items which can't have luck (jewels, feathers etc.) are ignored.
    /// </summary>
    /// <param name="items">The items which were put into the mix.</param>
    /// <returns>The created input.</returns>
    public static MixInput Create(IEnumerable<Item> items)
    {
        var equipment = items
            .Where(item => item.Definition?.PossibleItemOptions.Any(o => o.PossibleOptions.Any(p => p.OptionType == ItemOptionTypes.Luck)) ?? false)
            .Select(item => new EquipmentOptions(
                item.ItemOptions.Any(link => link.ItemOption?.OptionType == ItemOptionTypes.Luck),
                item.ItemOptions.FirstOrDefault(link => link.ItemOption?.OptionType == ItemOptionTypes.Option)?.Level ?? 0))
            .ToList();
        return new MixInput(equipment);
    }

    /// <summary>
    /// Rolls if the result gets luck, with the share of the input items which have luck.
    /// </summary>
    /// <returns><c>true</c>, if the result gets luck.</returns>
    public bool RollLuck()
    {
        return this.HasEquipment && Rand.NextInt(0, this._equipment.Count) < this._equipment.Count(e => e.HasLuck);
    }

    /// <summary>
    /// Rolls the level of the regular item option: the level of a random input item, 0 if it has no option.
    /// </summary>
    /// <returns>The option level.</returns>
    public int RollOptionLevel()
    {
        return this._equipment.SelectRandom()?.OptionLevel ?? 0;
    }

    private sealed record EquipmentOptions(bool HasLuck, int OptionLevel);
}
