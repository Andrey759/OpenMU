// <copyright file="PersonalDropTuningPlugInTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using System.ComponentModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.InMemory;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests for the <see cref="PersonalDropTuningPlugIn"/>.
/// </summary>
[TestFixture]
public class PersonalDropTuningPlugInTest
{
    private IContext _context = null!;

    /// <summary>
    /// Creates the in-memory persistence context.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._context = new InMemoryPersistenceContextProvider().CreateNewContext();
    }

    /// <summary>
    /// Disposes the persistence context.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this._context.Dispose();
    }

    /// <summary>
    /// Tests that normal equipment gets missing luck and regular options.
    /// </summary>
    [Test]
    public void NormalEquipmentGetsMissingLuckAndRegularOption()
    {
        var item = this.CreateNormalEquipment();
        var plugIn = new PersonalDropTuningPlugIn { Configuration = new PersonalDropTuningConfiguration { MinimumItemLevel = 5, MaximumItemLevel = 5, LuckChance = 1, MinimumOptionLevel = 3, MaximumOptionLevel = 3 } };

        plugIn.ItemDropping(null!, null!, item, new CancelEventArgs());

        Assert.That(item.ItemOptions.Count(link => link.ItemOption?.OptionType == ItemOptionTypes.Luck), Is.EqualTo(1));
        var regularOption = item.ItemOptions.Single(link => link.ItemOption?.OptionType == ItemOptionTypes.Option);
        Assert.That(regularOption.Level, Is.EqualTo(3));
        Assert.That(item.Level, Is.EqualTo(5));
    }

    /// <summary>
    /// Tests that existing options are retained and not duplicated.
    /// </summary>
    [Test]
    public void NormalEquipmentRetainsExistingOptionsWithoutDuplicates()
    {
        var item = this.CreateNormalEquipment();
        var luck = item.Definition!.PossibleItemOptions.SelectMany(definition => definition.PossibleOptions).First(option => option.OptionType == ItemOptionTypes.Luck);
        var regular = item.Definition.PossibleItemOptions.SelectMany(definition => definition.PossibleOptions).First(option => option.OptionType == ItemOptionTypes.Option);
        item.ItemOptions.Add(new BasicModel.ItemOptionLink { ItemOption = luck });
        item.ItemOptions.Add(new BasicModel.ItemOptionLink { ItemOption = regular, Level = 1 });
        item.HasSkill = true;
        var plugIn = new PersonalDropTuningPlugIn { Configuration = new PersonalDropTuningConfiguration { MinimumItemLevel = 5, MaximumItemLevel = 5, LuckChance = 1, MinimumOptionLevel = 3, MaximumOptionLevel = 3 } };

        plugIn.ItemDropping(null!, null!, item, new CancelEventArgs());

        Assert.That(item.ItemOptions.Count(link => link.ItemOption?.OptionType == ItemOptionTypes.Luck), Is.EqualTo(1));
        Assert.That(item.ItemOptions.Count(link => link.ItemOption?.OptionType == ItemOptionTypes.Option), Is.EqualTo(1));
        Assert.That(item.ItemOptions.Single(link => link.ItemOption?.OptionType == ItemOptionTypes.Option).Level, Is.EqualTo(3));
        Assert.That(item.HasSkill, Is.True);
    }

    /// <summary>
    /// Tests that excellent, ancient and non-equipment drops are left unchanged.
    /// </summary>
    [Test]
    public void SpecialAndNonEquipmentDropsAreNotModified()
    {
        var excellent = this.CreateNormalEquipment();
        excellent.ItemOptions.Add(new BasicModel.ItemOptionLink
        {
            ItemOption = new BasicModel.IncreasableItemOption { OptionType = ItemOptionTypes.Excellent },
        });
        var ancient = this.CreateNormalEquipment();
        ancient.ItemSetGroups.Add(new BasicModel.ItemOfItemSet { AncientSetDiscriminator = 1 });
        var nonEquipment = this.CreateNormalEquipment();
        nonEquipment.Definition!.Group = 12;
        var plugIn = new PersonalDropTuningPlugIn();

        foreach (var item in new[] { excellent, ancient, nonEquipment })
        {
            plugIn.ItemDropping(null!, null!, item, new CancelEventArgs());
        }

        Assert.That(excellent.ItemOptions.Count, Is.EqualTo(1));
        Assert.That(ancient.ItemOptions, Is.Empty);
        Assert.That(nonEquipment.ItemOptions, Is.Empty);
    }

    /// <summary>
    /// Tests that a failed luck roll removes luck which the default generator added.
    /// </summary>
    [Test]
    public void LuckIsRemovedWhenTheLuckRollFails()
    {
        var item = this.CreateNormalEquipment();
        var luck = item.Definition!.PossibleItemOptions.SelectMany(definition => definition.PossibleOptions).First(option => option.OptionType == ItemOptionTypes.Luck);
        item.ItemOptions.Add(new BasicModel.ItemOptionLink { ItemOption = luck });
        var plugIn = new PersonalDropTuningPlugIn { Configuration = new PersonalDropTuningConfiguration { LuckChance = 0 } };

        plugIn.ItemDropping(null!, null!, item, new CancelEventArgs());

        Assert.That(item.ItemOptions.Any(link => link.ItemOption?.OptionType == ItemOptionTypes.Luck), Is.False);
        Assert.That(item.Level, Is.InRange(3, 6));
    }

    /// <summary>
    /// Tests that a maximum option level chance of 1 always rolls the maximum option level.
    /// </summary>
    [Test]
    public void MaximumOptionLevelChanceRollsTheMaximumLevel()
    {
        var plugIn = new PersonalDropTuningPlugIn { Configuration = new PersonalDropTuningConfiguration { MinimumOptionLevel = 1, MaximumOptionLevel = 3, MaximumOptionLevelChance = 1 } };

        for (var i = 0; i < 20; i++)
        {
            var item = this.CreateNormalEquipment();
            plugIn.ItemDropping(null!, null!, item, new CancelEventArgs());
            Assert.That(item.ItemOptions.Single(link => link.ItemOption?.OptionType == ItemOptionTypes.Option).Level, Is.EqualTo(3));
        }
    }

    private TemporaryItem CreateNormalEquipment()
    {
        var itemDefinition = this._context.CreateNew<ItemDefinition>();
        itemDefinition.Group = 0;
        itemDefinition.Durability = 10;
        itemDefinition.MaximumItemLevel = 15;
        var optionDefinition = this._context.CreateNew<ItemOptionDefinition>();
        optionDefinition.PossibleOptions.Add(new BasicModel.IncreasableItemOption { OptionType = ItemOptionTypes.Luck });
        var regularOption = new BasicModel.IncreasableItemOption { OptionType = ItemOptionTypes.Option };
        regularOption.LevelDependentOptions.Add(new BasicModel.ItemOptionOfLevel { Level = 3 });
        optionDefinition.PossibleOptions.Add(regularOption);
        itemDefinition.PossibleItemOptions.Add(optionDefinition);
        return new TemporaryItem { Definition = itemDefinition };
    }
}
