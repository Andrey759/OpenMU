// <copyright file="MixInputTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.InMemory;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests for the <see cref="MixInput"/>, which passes the options of mixed equipment to the result.
/// </summary>
[TestFixture]
public class MixInputTest
{
    private const int Rolls = 50;

    private IContext _context = null!;

    private ItemOptionDefinition _optionDefinition = null!;

    /// <summary>
    /// Creates the in-memory persistence context and the options.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._context = new InMemoryPersistenceContextProvider().CreateNewContext();
        this._optionDefinition = this._context.CreateNew<ItemOptionDefinition>();
        this._optionDefinition.PossibleOptions.Add(new BasicModel.IncreasableItemOption { OptionType = ItemOptionTypes.Luck });
        this._optionDefinition.PossibleOptions.Add(new BasicModel.IncreasableItemOption { OptionType = ItemOptionTypes.Option });
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
    /// Tests that the result always gets luck and the option level, if all input items have them.
    /// </summary>
    [Test]
    public void AllInputItemsWithLuckAndOptionGiveThem()
    {
        var input = MixInput.Create([this.CreateEquipment(true, 3), this.CreateEquipment(true, 3)]);

        for (var i = 0; i < Rolls; i++)
        {
            Assert.That(input.RollLuck(), Is.True);
            Assert.That(input.RollOptionLevel(), Is.EqualTo(3));
        }
    }

    /// <summary>
    /// Tests that the result never gets luck or an option, if no input item has them.
    /// </summary>
    [Test]
    public void InputItemsWithoutLuckAndOptionGiveNothing()
    {
        var input = MixInput.Create([this.CreateEquipment(false, 0), this.CreateEquipment(false, 0)]);

        for (var i = 0; i < Rolls; i++)
        {
            Assert.That(input.RollLuck(), Is.False);
            Assert.That(input.RollOptionLevel(), Is.Zero);
        }
    }

    /// <summary>
    /// Tests that mixed option levels give one of the input levels.
    /// </summary>
    [Test]
    public void MixedOptionLevelsGiveOneOfThem()
    {
        var input = MixInput.Create([this.CreateEquipment(false, 1), this.CreateEquipment(false, 3), this.CreateEquipment(false, 3)]);

        var levels = Enumerable.Range(0, Rolls).Select(_ => input.RollOptionLevel()).ToList();

        Assert.That(levels, Is.All.AnyOf(1, 3));
    }

    /// <summary>
    /// Tests that items which can't have luck (e.g. jewels) are ignored.
    /// </summary>
    [Test]
    public void ItemsWithoutLuckOptionAreIgnored()
    {
        var jewelDefinition = this._context.CreateNew<ItemDefinition>();
        var jewel = new TemporaryItem { Definition = jewelDefinition };

        Assert.That(MixInput.Create([jewel]).HasEquipment, Is.False);
        Assert.That(MixInput.Create([jewel, this.CreateEquipment(true, 2)]).RollLuck(), Is.True);
    }

    private TemporaryItem CreateEquipment(bool luck, int optionLevel)
    {
        var definition = this._context.CreateNew<ItemDefinition>();
        definition.PossibleItemOptions.Add(this._optionDefinition);
        var item = new TemporaryItem { Definition = definition };
        if (luck)
        {
            item.ItemOptions.Add(new BasicModel.ItemOptionLink { ItemOption = this._optionDefinition.PossibleOptions.First(o => o.OptionType == ItemOptionTypes.Luck) });
        }

        if (optionLevel > 0)
        {
            item.ItemOptions.Add(new BasicModel.ItemOptionLink { ItemOption = this._optionDefinition.PossibleOptions.First(o => o.OptionType == ItemOptionTypes.Option), Level = optionLevel });
        }

        return item;
    }
}
