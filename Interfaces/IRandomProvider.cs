namespace FactionsAtTheEnd.Interfaces;

/// <summary>
/// Provides an abstraction for random number generation.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// Returns a non-negative random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
    /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue.</returns>
    int Next(int minValue, int maxValue);

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue.</returns>
    int Next(int maxValue);

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than Int32.MaxValue.</returns>
    int Next();

    /// <summary>
    /// Returns a random floating-point number between 0.0 and 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
    double NextDouble();
}
