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

    internal static int ReadMenu(string prompt, string[] options,
        string? optionPrompt = null, bool newline = true)
    {
        Console.Clear();
        // Print prompt.
        if (!string.IsNullOrEmpty(prompt))
        {
            Console.WriteLine(prompt);
            if (newline)
                Console.WriteLine();
        }
        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine($" {i + 1}) {options[i]}");
        }

        Console.WriteLine();
        int lineNo = Console.CursorTop;
        string buffer = "";
        bool inputSuccess;
        int opt;
        while (true)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = lineNo;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorLeft = 0;
            Console.Write(optionPrompt ?? $"Enter option: ");
            Console.Write(buffer);
            var key = Console.ReadKey(true);

            void setBuffer(int num)
            {
                buffer = Math.Clamp(num, 1, options.Length).ToString();
            }

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return -1;

                case ConsoleKey.Backspace:
                    if (buffer.Length > 0)
                        buffer = buffer[..^1];
                    break;

                case ConsoleKey.UpArrow:
                    _ = int.TryParse(buffer, out opt);
                    setBuffer(--opt);
                    break;

                case ConsoleKey.DownArrow:
                    _ = int.TryParse(buffer, out opt);
                    setBuffer(++opt);
                    break;

                case ConsoleKey.Enter:
                    inputSuccess = int.TryParse(buffer, out opt);
                    buffer = "";
                    if (inputSuccess && opt > 0 && opt <= options.Length)
                    {
                        return --opt;
                    }
                    break;

                default:
                    if (char.IsDigit(key.KeyChar))
                    {
                        buffer += key.KeyChar;
                    }
                    break;
            }
        }
    }

    internal static string ReadString(string? prompt = null)
    {
        while (true)
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                Console.WriteLine(prompt);
            }
            string? result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                Console.WriteLine("Invalid input, please try again.\n");
            }
            else
            {
                return result;
            }
        }
    }

    internal static int ReadInt(string? prompt = null)
    {
        while (true)
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                Console.WriteLine(prompt);
            }
            if (int.TryParse(Console.ReadLine(), out int res))
            {
                return res;
            }
            else
            {
                Console.WriteLine("Invalid input, please try again.\n");
            }
        }
    }

    internal static T? ReadMenu<T>(string prompt, T[] values, Func<T, string> convert) where T : class
    {
        string[] options = [.. values.Select(convert)];
        int result = ReadMenu(prompt, options);
        if (result >= 0 && result < values.Length)
            return values[result];
        else
            return null;
    }

    internal static void Prompt(string prompt)
    {
        Console.Clear();
        if (!string.IsNullOrEmpty(prompt))
        {
            Console.WriteLine(prompt);
        }
        Console.Write("Press any key to continue ");
        Console.ReadKey();
    }

    internal delegate void MenuCallback();

    internal static bool Menu(string prompt, (string, MenuCallback?)[] options,
        string? optionPrompt = null, bool newline = true)
    {
        string[] opts = [.. options.Select(o => o.Item1)];
        MenuCallback?[] callbacks = [.. options.Select(o => o.Item2)];
        int result = ReadMenu(prompt, opts, optionPrompt, newline);
        if (result >= 0 && result < callbacks.Length)
        {
            MenuCallback? callback = callbacks[result];
            if (callback is not null)
            {
                callback();
                return false;
            }
        }
        return true;
    }

    public static void WriteHighlighted(string value, SearchIndex[] indices,
        bool newline = true, ConsoleColor highlight = ConsoleColor.Blue,
        ConsoleColor? normal = null)
    {
        void printHighlight(string v)
        {
            Console.ForegroundColor = highlight;
            Console.Write(v);
            if (normal is null)
                Utils.ResetForeground();
            else
                Console.ForegroundColor = (ConsoleColor)normal;
        }

        if (indices.Length < 1)
        {
            Console.Write(value);
            if (newline)
                Console.WriteLine();
            return;
        }

        for (int i = 0, j = 0; i < value.Length;)
        {
            SearchIndex index = indices[j];

            Console.Write(value[i..index.Start]);
            i = index.Start;

            printHighlight(value[index.Start..index.End]);
            i = index.End;

            if (j < indices.Length - 1)
            {
                j++;
            }
            else
            {
                Console.Write(value[i..^0]);
                if (newline)
                    Console.WriteLine();
                i = value.Length;
            }
        }
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
