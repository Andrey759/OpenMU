// <copyright file="PersonalDropTuningPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.ComponentModel;
using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Personal server drop tuning: normal equipment dropped by monsters gets a random item level,
/// a configurable chance for luck and always a regular option with a random level.
/// Filtering drops by the killer's class family happens in <see cref="DefaultDropGenerator"/>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.PersonalDropTuningPlugIn_Name), Description = nameof(PlugInResources.PersonalDropTuningPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C92A20B4-64AE-4B56-9927-26102A0D496E")]
public sealed class PersonalDropTuningPlugIn : IFeaturePlugIn, IMonsterItemDropPlugIn,
    ISupportCustomConfiguration<PersonalDropTuningConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    /// <summary>
    /// The highest equipment item group. Item group semantics are part of the game data model, not a drop tuning setting.
    /// </summary>
    internal const byte MaximumEquipmentItemGroup = 11;

    /// <inheritdoc />
    public PersonalDropTuningConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public object CreateDefaultConfig()
    {
        return new PersonalDropTuningConfiguration();
    }

    /// <inheritdoc />
    public void ItemDropping(AttackableNpcBase monster, Player killer, Item item, CancelEventArgs eventArgs)
    {
        if (item.Definition is not { } definition
            || definition.Group > MaximumEquipmentItemGroup
            || item.IsExcellent()
            || item.IsAncient())
        {
            return;
        }

        var config = this.Configuration ??= new PersonalDropTuningConfiguration();
        var minimumLevel = Math.Min(config.MinimumItemLevel, config.MaximumItemLevel);
        item.Level = (byte)Math.Min(Rand.NextInt(minimumLevel, config.MaximumItemLevel + 1), definition.MaximumItemLevel);
        item.Durability = item.GetMaximumDurabilityOfOnePiece();

        SetLuck(item, definition, Rand.NextRandomBool(config.LuckChance));

        SetRegularOption(item, definition, RollOptionLevel(config));
    }

    private static byte RollOptionLevel(PersonalDropTuningConfiguration config)
    {
        var minimumOptionLevel = Math.Min(config.MinimumOptionLevel, config.MaximumOptionLevel);
        if (config.MaximumOptionLevelChance <= 0 || minimumOptionLevel == config.MaximumOptionLevel)
        {
            return (byte)Rand.NextInt(minimumOptionLevel, config.MaximumOptionLevel + 1);
        }

        return Rand.NextRandomBool(config.MaximumOptionLevelChance)
            ? config.MaximumOptionLevel
            : (byte)Rand.NextInt(minimumOptionLevel, config.MaximumOptionLevel);
    }

    private static void SetLuck(Item item, ItemDefinition definition, bool hasLuck)
    {
        var existingLuck = item.ItemOptions.FirstOrDefault(link => link.ItemOption?.OptionType == ItemOptionTypes.Luck);
        if (!hasLuck)
        {
            if (existingLuck is not null)
            {
                item.ItemOptions.Remove(existingLuck);
            }

            return;
        }

        if (existingLuck is not null)
        {
            return;
        }

        var luckOption = definition.PossibleItemOptions
            .SelectMany(optionDefinition => optionDefinition.PossibleOptions)
            .FirstOrDefault(option => option.OptionType == ItemOptionTypes.Luck);
        if (luckOption is not null)
        {
            item.ItemOptions.Add(new ItemOptionLink { ItemOption = luckOption });
        }
    }

    private static void SetRegularOption(Item item, ItemDefinition definition, byte optionLevel)
    {
        if (optionLevel == 0)
        {
            return;
        }

        var existingOption = item.ItemOptions.FirstOrDefault(link =>
            link.ItemOption?.OptionType == ItemOptionTypes.Option
            && !link.ItemOption.IsFixedOption());
        if (existingOption is not null)
        {
            existingOption.Level = optionLevel;
            return;
        }

        var regularOption = definition.PossibleItemOptions
            .SelectMany(optionDefinition => optionDefinition.PossibleOptions)
            .FirstOrDefault(option => option.OptionType == ItemOptionTypes.Option && !option.IsFixedOption());
        if (regularOption is not null)
        {
            item.ItemOptions.Add(new ItemOptionLink { ItemOption = regularOption, Level = optionLevel });
        }
    }
}
