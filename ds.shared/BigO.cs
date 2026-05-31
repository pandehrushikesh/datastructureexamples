namespace ds.shared;

/// <summary>
/// Prints the time complexity and a short explanation for any data structure operation.
/// Referenced by every ds.*.example project so the format stays consistent across the solution.
/// </summary>
public static class BigO
{
    /// <summary>
    /// Prints the Big O notation and a one-to-two sentence explanation to the console.
    /// </summary>
    /// <param name="notation">e.g. "O(1)", "O(n)", "O(n²)", "O(n log n)"</param>
    /// <param name="explanation">Why the operation has that complexity.</param>
    public static void Print(string notation, string explanation)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"  ⏱  Complexity: {notation}");
        Console.ResetColor();
        Console.WriteLine($"     {explanation}");
    }
}
