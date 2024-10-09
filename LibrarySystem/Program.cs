using System.Text.Json;

namespace LibrarySystem;

internal static class Program
{
    internal static Library library = new();
    internal static string libraryPath = Path.Combine(
        Environment.CurrentDirectory, "library.json");
    internal static JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
    };

    internal static void Main(string[] args)
    {
        if (File.Exists(libraryPath))
        {
            // Load the saved library file containing all the books and users.
            LoadLibrary();
        }

        while (true)
        {
            int res = ReadMenu(
                prompt: "Welcome to the Library system.",
                options: [
                    "Create new user account",
                    "Login as user account",
                    "Login as admin user",
                    "Exit program"
                ]
            );

            switch (res)
            {
                case 0:
                    CreateUser();
                    break;
                case 1:
                    SelectUser();
                    break;
                case 2:
                    AdminAccount();
                    break;
                case 3:
                    goto ExitProgram;
                default:
                    goto ExitProgram;
            }
        }
    ExitProgram:
        SaveLibrary();
    }

    internal static void SaveLibrary()
    {
        using FileStream file = File.Open(libraryPath, FileMode.Create);
        JsonSerializer.Serialize(file, library, options);
    }

    internal static void LoadLibrary()
    {
        using FileStream file = File.Open(libraryPath, FileMode.Open);
        Library? lib = JsonSerializer.Deserialize<Library>(file, options);
        if (lib is not null)
            library = lib;

        // Restore user books, removed as they produced duplicate objects.
        foreach (User user in library.Users)
        {
            user.Books.Clear();
            foreach (int uniqueId in user.UniqueBookIds)
            {
                Book? book = library.FindBookByUnqiueId(uniqueId);
                if (book is not null)
                {
                    if (book.UserId == user.UserId)
                        user.Books.Add(book);
                    else
                        throw new Exception(
                            "Error: book did not match its user id.");
                }
            }
        }
    }

    internal static void CreateUser()
    {
        Console.Clear();
        Console.WriteLine("Create a new user");
        string name = ReadString("Please enter your name:");
        string email = ReadString("Please enter your email address:");
        library.AddUser(name, email);
        Console.WriteLine($"User {name} added successfully.");
    }

    internal static void SelectUser()
    {
        User[] users = [.. library.Users];
        User? currentUser = ReadMenu("Please select from the list of users:",
            users, u => $"{u.UserId}, {u.Name} ({u.Email})");

        if (currentUser is not null)
            UserAccount(currentUser);
    }

    internal static void UserAccount(User user)
    {
        while (true)
        {
            int res = ReadMenu(
                $"Welcome, {user.Name}\nWhat would you like to do today?\n",
                [
                    "View my borrowed books",
                    "Borrow some new books",
                    "Return borrowed books",
                    "Make a payment",
                    "Logout"
                ]
            );

            switch (res)
            {
                case 0:
                    {
                        // Get list of books and order by author, then title.
                        IEnumerable<Book> books = user.Books
                            .OrderBy(b => b.Author)
                            .ThenBy(b => b.Title);

                        string prompt = "";

                        if (books.Any())
                        {
                            prompt = "You are currently borrowing the following books:\n";
                            foreach (Book book in books)
                            {
                                prompt += $" - Due {book.DueDate}, {book.Title}, {book.Author}\n";
                            }
                        }
                        else
                        {
                            prompt = "You are not currently borrowing any books.\n";
                        }

                        Prompt(prompt);
                        break;
                    }

                case 1:
                    {
                        IEnumerable<Book> books = InteractiveSearch();
                        if (books.Any())
                        {
                            try
                            {
                                user.BorrowBooks(books);
                                Prompt("Books borrowed successfully.");
                            }
                            catch (Exception e)
                            {
                                Prompt(
                                    "You are unable to borrow these books."
                                        + $"\nError: {e.Message} \n");
                            }
                        }
                        break;
                    }

                case 2:
                    {
                        break;
                    }

                case 3:
                    return;

                case 4:
                    return;

                default:
                    return;
            }
        }
    }

    internal static IEnumerable<Book> InteractiveSearch()
    {
        Console.Clear();
        Console.WriteLine("Enter some characters to begin searching.");

        string query = "";

        // Cursor position, relative to left-most column.
        int left = 0;
        int selected = -1;
        int scroll = 0;
        List<Book> selection = [];
        List<Book> results = [];

    SearchLoop:
        while (true)
        {
            int maxHeight = Console.BufferHeight - 2;
            int bottom = Console.WindowHeight - 1;
            int offsetBtm = scroll + maxHeight;
            if (selected > results.Count)
                selected = -1;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Interactive Search: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "Up/Down: Scroll, Enter: Select, Esc: Clear/Exit");
            Utils.ResetForeground();


            if (query.Length > 0)
            {
                results = [.. library.SearchByKeyword(query)];
                for (int i = scroll; i < results.Count && i < offsetBtm; i++)
                {
                    Book book = results[i];
                    string bookInfo = $"{book.Title}, {book.Author}";
                    SearchIndex[] indices = Utils.FuzzySearchDetailed(
                        bookInfo, query);

                    if (selected == i)
                        Console.BackgroundColor = ConsoleColor.DarkGray;

                    Console.Write(selection.Contains(book) ? " [x] " : " [ ] ");
                    WriteHighlighted(bookInfo, indices);
                    Utils.ResetBackground();
                }
            }
            else
            {
                results = library.Copies;
                for (int i = scroll; i < results.Count && i < offsetBtm; i++)
                {
                    Book book = results[i];
                    string bookInfo = $"{book.Title}, {book.Author}";
                    if (selected == i)
                        Console.BackgroundColor = ConsoleColor.DarkGray;

                    Console.Write(selection.Contains(book) ? " [x] " : " [ ] ");
                    Console.WriteLine(bookInfo);
                    Utils.ResetBackground();
                }
            }

            // Print the prompt and input query.
            Console.CursorTop = bottom;

            if (query.Length < 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Start typing to search for a title...");
                Utils.ResetForeground();
            }
            else
            {
                Console.Write(query);
            }

            Console.CursorLeft = left;
            ConsoleKeyInfo key = Console.ReadKey(true);

            // Input processing.
            switch (key.Key)
            {
                // Exit search mode.
                case ConsoleKey.Escape:
                    if (query.Length == 0)
                        goto Confirmation;
                    query = "";
                    left = 0;
                    break;

                // Delete char infront of cursor.
                case ConsoleKey.Delete:
                    if (left < query.Length)
                    {
                        scroll = 0;
                        selected = -1;
                        query = query.Remove(left, 1);
                        // Delete, clear next char, then go back one char.
                        Console.Write(" \b");
                    }
                    break;

                // Delete char behind cursor.
                case ConsoleKey.Backspace:
                    if (left > 0)
                    {
                        scroll = 0;
                        selected = -1;
                        query = query.Remove(left - 1, 1);
                        // Backspace. Go back one char, erase char with space
                        // (advances cursor forward), then go back again.
                        Console.Write("\b \b");
                        left--;
                    }
                    break;

                case ConsoleKey.Enter:
                    if (selected < 0 || selected > results.Count)
                        break;
                    if (selection.Contains(results[selected]))
                        selection.Remove(results[selected]);
                    else if (selection.Count < 3)
                        selection.Add(results[selected]);
                    break;

                // Move cursor to the left.
                case ConsoleKey.LeftArrow:
                    if (left > 0)
                        left--;
                    break;

                // Move cursor to the right.
                case ConsoleKey.RightArrow:
                    if (left < query.Length)
                        left++;
                    break;

                case ConsoleKey.UpArrow:
                    if (selected > 0)
                        selected--;
                    if (scroll > 0 && (selected - 1 < offsetBtm - maxHeight))
                        scroll--;
                    break;

                case ConsoleKey.DownArrow:
                    if (selected < results.Count - 1)
                        selected++;
                    if ((scroll < results.Count) && (selected + 2 > offsetBtm))
                        scroll++;
                    break;

                // Go to beginning of line.
                case ConsoleKey.Home:
                    left = 0;
                    break;

                // Go to end of line.
                case ConsoleKey.End:
                    left = query.Length;
                    break;

                default:
                    // Only add printable characters to search query.
                    if (!char.IsControl(key.KeyChar))
                    {
                        scroll = 0;
                        selected = -1;
                        query = query.Insert(left, key.KeyChar.ToString());
                        left++;
                    }
                    break;
            }
        }

    Confirmation:
        string prompt = "";

        if (selection.Count > 0)
        {
            prompt = "You have selected to borrow the following books:\n";
            foreach (Book book in selection)
            {
                prompt += $" - {book.Title}, {book.Author}\n";
            }
        }
        else
        {
            prompt = "You have not selected any books.\n";
        }


        int result = ReadMenu(prompt, [
            "Continue",
            "Back to search"
        ]);

        switch (result)
        {
            case 0:
                break;
            case 1:
                goto SearchLoop;
        }

        return selection;
    }

    public static void SearchSelectionConfirm(List<Book> books)
    {

    }

    public static void WriteHighlighted(string value, SearchIndex[] indices,
        bool newline = true)
    {
        static void highlight(string v)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(v);
            Utils.ResetForeground();
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

            highlight(value[index.Start..index.End]);
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

    internal static IEnumerable<Book> SelectBooks()
    {
        List<Book> books = [];
        while (true)
        {
            int res = ReadMenu(
                prompt: "Select books:",
                options: [
                    "Search by title",
                    "Search by author",
                    "Search by keyword",
                    "Continue with books",
                    "Cancel search"
                ]
            );

            switch (res)
            {
                case 0:
                    {
                        string query = ReadString("");
                        break;
                    }

                case 1:
                    break;

                case 2:
                    break;

                case 3:
                    return books;

                case 4:
                    return [];
            }
        }
    }

    internal static void AdminAccount()
    {

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

    internal static void Prompt(string prompt) => ReadMenu(prompt, ["OK"]);

    internal static int ReadMenu(string prompt, string[] options)
    {
        int promptHeight = prompt.Split('\n').Length;
        Console.Clear();
        int selected = -1;
        int scroll = 0;

        while (true)
        {
            int maxHeight = Console.WindowHeight - 1 - promptHeight;
            int offsetBtm = scroll + maxHeight;

            Console.Clear();
            Console.Write($"{prompt}\n");
            Utils.ResetForeground();

            {
                for (int i = scroll; i < options.Length && i < offsetBtm; i++)
                {
                    if (selected == i)
                        Console.BackgroundColor = ConsoleColor.DarkGray;

                    Console.WriteLine($" {i + 1}) {options[i]}");

                    Utils.ResetBackground();
                }
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            // Input processing.
            switch (key.Key)
            {
                // Cancel prompt.
                case ConsoleKey.Escape:
                    return -1;

                case ConsoleKey.Enter:
                    if (selected >= 0 && selected < options.Length)
                        return selected;
                    break;

                case ConsoleKey.UpArrow:
                    if (selected > 0)
                        selected--;
                    if (scroll > 0 && (selected - 1 < offsetBtm - maxHeight))
                        scroll--;
                    break;

                case ConsoleKey.DownArrow:
                    if (selected < options.Length - 1)
                        selected++;
                    if ((scroll < options.Length) && (selected + 2 > offsetBtm))
                        scroll++;
                    break;

                // Go to first option.
                case ConsoleKey.Home:
                    selected = 0;
                    scroll = 0;
                    break;

                // Go to end last option.
                case ConsoleKey.End:
                    selected = options.Length - 1;
                    scroll = Math.Max(options.Length - maxHeight + 1, 0);
                    break;

                default:
                    // If user presses a digit key, then use that as a shortcut
                    // for an option.
                    if (char.IsDigit(key.KeyChar))
                    {
                        scroll = 0;
                        int idx = int.Parse(key.KeyChar.ToString());
                        selected = idx - 1;
                    }
                    break;
            }
        }
    }
}
