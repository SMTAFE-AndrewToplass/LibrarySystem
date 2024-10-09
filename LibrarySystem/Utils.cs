namespace LibrarySystem;

public static class Utils
{
    /// <summary>
    /// Case-Insensitive char equals extension method.
    /// </summary>
    /// <param name="a">The first char to compare</param>
    /// <param name="b">The second char to compare</param>
    /// <returns>True if the two chars match, false if not.</returns>
    public static bool EqualsInsensitive(this char a, char b) =>
        char.ToLower(a) == char.ToLower(b);

    public static bool FuzzySearchContains(string search, string query)
    {
        string q = query.Replace(" ", null);

        if (q.Length > search.Length)
            return false;

        int j = 0;
        for (int i = 0; i < search.Length && j < q.Length; i++)
        {
            if (search[i].EqualsInsensitive(q[j]))
            {
                // Increments `i` and `j` until they no longer match or either
                // array indices are out of bounds.
                for (; i < search.Length && j < q.Length
                    && search[i].EqualsInsensitive(q[j]); i++, j++) ;
            }
        }
        return j == q.Length;
    }

    public static SearchIndex[] FuzzySearchDetailed(string search, string query)
    {
        List<SearchIndex> indices = [];

        string q = query.Replace(" ", null);

        if (q.Length > search.Length)
            return [];

        // `i` is the index of `search`, `j` is the index of `q`.
        for (int i = 0, j = 0; i < search.Length && j < q.Length; i++)
        {
            // Loop increments `i` until the character at index `i` and index
            // `j` match, when they do match, increment both until they no
            // longer match.
            if (search[i].EqualsInsensitive(q[j]))
            {
                // Store the index of matching character inside `search`.
                int start = i;

                // Increments `i` and `j` until they no longer match or either
                // array indices are out of bounds.
                for (; i < search.Length && j < q.Length
                    && search[i].EqualsInsensitive(q[j]); i++, j++) ;

                // Store the index after the last matching character inside
                // `search`.
                int end = i;

                indices.Add(new() { Start = start, End = end });
            }
        }

        return [.. indices];
    }

    public static int FuzzySearchOrder(string search, string query)
    {
        int matches = 0;
        string q = query.Replace(" ", null);
        if (q.Length > search.Length)
            return 0;

        // `i` is the index of `search`, `j` is the index of `q`.
        for (int i = 0, j = 0; i < search.Length && j < q.Length; i++)
        {
            // Loop increments `i` until the character at index `i` and index
            // `j` match, when they do match, increment both until they no
            // longer match.
            if (search[i].EqualsInsensitive(q[j]))
            {
                // Store the index of matching character inside `search`.
                int start = i;

                // Increments `i` and `j` until they no longer match or either
                // array indices are out of bounds.
                for (; i < search.Length && j < q.Length
                    && search[i].EqualsInsensitive(q[j]); i++, j++) ;

                // Store the index after the last matching character inside
                // `search`.
                int end = i;

                matches += end - start;
            }
        }

        return matches;
    }

    public static void ResetForeground()
    {
        ConsoleColor bg = Console.BackgroundColor;
        Console.ResetColor();
        Console.BackgroundColor = bg;
    }

    public static void ResetBackground()
    {
        ConsoleColor fg = Console.ForegroundColor;
        Console.ResetColor();
        Console.ForegroundColor = fg;
    }
}

public readonly struct SearchIndex
{
    public int Start { get; init; }
    public int End { get; init; }
    public readonly int Length { get => End - Start; }
    public override string ToString()
    {
        return $"({Start}, {End})";
    }
}
