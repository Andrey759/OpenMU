// <copyright file="CraftingResultClassSelection.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;

/// <summary>
/// Selects the crafted wings (or chaos weapon) for a character class instead of a random one:
/// the result keeps the class of the mixed class item (chaos weapon for the first wings, first wings for the second
/// wings) and, among those, the class of the crafting character.
/// </summary>
internal static class CraftingResultClassSelection
{
    /// <summary>
    /// Selects the result item.
    /// </summary>
    /// <param name="results">The possible results.</param>
    /// <param name="requiredItems">The required items of the mix.</param>
    /// <param name="player">The crafting player.</param>
    /// <returns>The selected result.</returns>
    public static ItemCraftingResultItem Select(ICollection<ItemCraftingResultItem> results, IList<CraftingRequiredItemLink> requiredItems, Player player)
    {
        var withDefinition = results.Where(r => r.ItemDefinition is not null).ToList();
        if (withDefinition.Count == 0)
        {
            return results.SelectRandom()!;
        }

        // Class items in the mix: required items which are a specific list of class items (chaos weapons, wings).
        var sourceClasses = requiredItems
            .Where(link => link.ItemRequirement.PossibleItems.Any()
                           && link.ItemRequirement.PossibleItems.All(d => d.QualifiedCharacters.Any()))
            .SelectMany(link => link.Items)
            .SelectMany(item => item.Definition?.QualifiedCharacters ?? Enumerable.Empty<CharacterClass>())
            .ToHashSet();
        var playerClasses = GetClassFamily(player.SelectedCharacter?.CharacterClass);

        bool FitsSource(ItemCraftingResultItem r) => r.ItemDefinition!.QualifiedCharacters.Any(sourceClasses.Contains);
        bool FitsPlayer(ItemCraftingResultItem r) => r.ItemDefinition!.QualifiedCharacters.Any(playerClasses.Contains);

        var candidates = withDefinition.Where(r => FitsSource(r) && FitsPlayer(r)).ToList();
        if (candidates.Count == 0)
        {
            candidates = withDefinition.Where(FitsSource).ToList();
        }

        if (candidates.Count == 0)
        {
            candidates = withDefinition.Where(FitsPlayer).ToList();
        }

        return candidates.Count > 0 ? candidates.SelectRandom()! : results.SelectRandom()!;
    }

    /// <summary>
    /// Gets the class and its next generations (Dark Wizard: Soul Master, Grand Master), because items of a later
    /// generation belong to the same character.
    /// </summary>
    private static HashSet<CharacterClass> GetClassFamily(CharacterClass? characterClass)
    {
        var family = new HashSet<CharacterClass>();
        for (var c = characterClass; c is not null && family.Add(c); c = c.NextGenerationClass)
        {
        }

        return family;
    }
}
