using FactionsAtTheEnd.Interfaces;

namespace FactionsAtTheEnd.Providers;

/// <summary>
/// Default implementation of IRandomProvider using System.Random.
/// </summary>
public class RandomProvider : IRandomProvider
{
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomProvider"/> class.
    /// </summary>
    public RandomProvider() { }

    /// <inheritdoc/>
    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    /// <inheritdoc/>
    public int Next(int maxValue) => _random.Next(maxValue);

    /// <inheritdoc/>
    public int Next() => _random.Next();

    /// <inheritdoc/>
    public double NextDouble() => _random.NextDouble();
}
