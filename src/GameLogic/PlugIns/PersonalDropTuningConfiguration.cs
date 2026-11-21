// <copyright file="PersonalDropTuningConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// Configuration of the <see cref="PersonalDropTuningPlugIn"/>.
/// </summary>
public class PersonalDropTuningConfiguration
{
    /// <summary>
    /// Gets or sets the minimum item level (+N) of normal equipment dropped by monsters.
    /// </summary>
    public byte MinimumItemLevel { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum item level (+N) of normal equipment dropped by monsters.
    /// </summary>
    public byte MaximumItemLevel { get; set; } = 6;

    /// <summary>
    /// Gets or sets the chance (0..1) that dropped normal equipment has luck.
    /// </summary>
    public double LuckChance { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the minimum regular option level (1 = +4, 2 = +8, 3 = +12; 0 = no forced option).
    /// </summary>
    public byte MinimumOptionLevel { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum regular option level (1 = +4, 2 = +8, 3 = +12).
    /// </summary>
    public byte MaximumOptionLevel { get; set; } = 3;

    /// <summary>
    /// Gets or sets the chance of the maximum option level. The other levels share the rest evenly.
    /// 0 means that all levels are equally likely.
    /// </summary>
    public double MaximumOptionLevelChance { get; set; }
}
