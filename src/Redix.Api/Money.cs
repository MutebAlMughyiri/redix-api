namespace Redix.Api;

/// <summary>What every amount a person types shares: the two decimals of the riyal, and the largest amount the column and the app can show (NFR-04).</summary>
public static class Money
{
    /// <summary>The largest amount, either way.</summary>
    public const decimal Largest = 999_999_999_999_999.99m;

    /// <summary>Whether <paramref name="amount"/> is within <see cref="Largest"/> and has at most two decimals.</summary>
    public static bool IsAcceptable(decimal amount) => Math.Abs(amount) <= Largest && decimal.Round(amount, 2) == amount;
}
