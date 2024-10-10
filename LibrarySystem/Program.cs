using System.Text.Json;
using static System.Reflection.Metadata.BlobBuilder;

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
                $"Welcome {user.Name},\nWhat would you like to do today?",
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
                // view borrowed books
                case 0:
                    {
                        // Get list of books and order by author, then title.
                        IEnumerable<Book> books = user.Books
                            .OrderBy(b => b.Author)
                            .ThenBy(b => b.Title);

                        string prompt = "";

                        if (books.Any())
                        {
                            prompt = "You are currently borrowing the following books:\n\n";
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

                // Borrow a new book.
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
                                    "Unable to borrow these books."
                                        + $"\n  Error: {e.Message} \n");
                            }
                        }
                        break;
                    }

                // Return books
                case 2:
                    {
                        // Get list of books and order by author, then title.
                        IEnumerable<Book> books = user.Books
                            .OrderBy(b => b.Author)
                            .ThenBy(b => b.Title);

                        string prompt = "";

                        if (books.Any())
                        {
                            prompt = "You are currently borrowing the following books:\n\n";
                            foreach (Book book in books)
                            {
                                prompt += $" - Due {book.DueDate}, {book.Title}, {book.Author}\n";
                            }
                            prompt += "\nWould you like to return these books?";

                            double fees = books.Aggregate(0.0, (a, b) => a + user.GetLateFee(b));
                            if (fees > 0)
                            {
                                prompt += $"It will add ${fees:.00} in late fees.";
                            }

                            res = ReadMenu(prompt, ["Yes", "No"], newline: true);
                            if (res == 0)
                            {
                                try
                                {
                                    user.ReturnBooks();
                                    Prompt("Books returned successfully");
                                }
                                catch (Exception e)
                                {
                                    Prompt(
                                    "Unable to return books."
                                        + $"\n  Error: {e.Message} \n");
                                }
                            }
                            break;
                        }
                        else
                        {
                            prompt = "You are not currently borrowing any books.\n";
                            Prompt(prompt);
                            break;
                        }
                    }

                // Make a payment
                case 3:
                    {
                        string prompt = "";

                        if (user.FeesOwed > 0)
                        {
                            double fees = user.FeesOwed;
                            prompt = $"You currently owe ${user.FeesOwed:.00} in late fees.\n\n";
                            prompt += "Would you like to pay your outstanding fees?";

                            res = ReadMenu(prompt, ["Yes", "No"], newline: true);
                            if (res == 0)
                            {
                                user.PayFee(fees);
                                Prompt("Payment successful.");
                            }

                            break;
                        }
                        else
                        {
                            prompt = "You do have any outstanding fees.\n";
                            Prompt(prompt);
                            break;
                        }
                    }

                // logout / cancel
                case 4:
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
        List<Book> selection = [];

    SearchLoop:
        while (true)
        {
            List<Book> results;
            int bottom = Console.WindowHeight - 1;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Interactive Search: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "Up/Down: Scroll, Enter: Select, Esc: Clear/Exit | Red: Unavailable");
            Utils.ResetForeground();

            if (query.Length > 0)
            {
                results = [.. library.SearchByKeyword(query, library.AvailableCopies)];
                for (int i = 0; i < results.Count; i++)
                {
                    Book book = results[i];
                    string bookInfo = $"{book.Title}, {book.Author} ";
                    SearchIndex[] indices = Utils.FuzzySearchDetailed(
                        bookInfo, query);

                    if (selected == i)
                        Console.BackgroundColor = ConsoleColor.DarkGray;

                    Console.Write(selection.Contains(book) ? " [x] " : " [ ] ");

                    WriteHighlighted(bookInfo, indices,
                        normal: book.IsAvailable ? null : ConsoleColor.Red);

                    Utils.ResetBackground();
                }
            }
            else
            {
                results = library.AvailableCopies;
                for (int i = 0; i < results.Count; i++)
                {
                    Book book = results[i];
                    string bookInfo = $"{book.Title}, {book.Author} ";
                    if (selected == i)
                        Console.BackgroundColor = ConsoleColor.DarkGray;

                    if (!book.IsAvailable)
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write(selection.Contains(book) ? " [x] " : " [ ] ");
                    Console.WriteLine(bookInfo);
                    Console.ResetColor();
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
                    break;

                case ConsoleKey.DownArrow:
                    if (selected < results.Count - 1)
                        selected++;
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
                        selected = -1;
                        query = query.Insert(left, key.KeyChar.ToString());
                        left++;
                    }
                    break;
            }
        }

    Confirmation:
        string prompt = "You have not selected any books.\n";

        if (selection.Count > 0)
        {
            prompt = "You have selected to borrow the following books:\n\n";
            foreach (Book book in selection)
            {
                prompt += $" - {book.Title}, {book.Author}\n";
            }
        }

        int result = ReadMenu(prompt, [
            "Borrow books",
            "Back to search",
            "Cancel"
        ], newline: false);

        switch (result)
        {
            case 0:
                break;
            case 1:
                goto SearchLoop;
            case 2:
            default:
                return [];
        }

        return selection;
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
        static string bookDisplay(Book book)
        {
            User? user = library.FindUserById(book.UserId);
            string userInfo = "";
            if (user is not null)
            {
                userInfo = $", borrowed by {user.Name} ({user.UserId})";
            }
            return $"{book.UniqueId}: {book.Title}, {book.Author}{userInfo}";
        }
        static string userDisplay(User user) =>
            $"{user.UserId}, {user.Name} ({user.Email})";

        while (true)
        {
            int res = ReadMenu(
                prompt: "Select books:",
                options: [
                    "View all books",
                    "View all users",
                    "View all borrowed books",
                    "View all borrowing users",
                    "Logout"
                ]
            );

            switch (res)
            {
                // View all books
                case 0:
                    {
                        Book[] books = [.. library.Books];
                        ReadMenu("All books in the library", books, bookDisplay);
                        break;
                    }

                // View all users
                case 1:
                    {
                        User[] users = [.. library.Users];
                        User? currentUser = ReadMenu("All users in the library",
                            users, userDisplay);

                        if (currentUser is not null)
                            UserAccount(currentUser);
                        break;
                    }

                // View all borrowed books
                case 2:
                    {
                        Book[] books = [.. library.GetBorrowedBooks()];
                        ReadMenu("All borrowed books in the library", books, bookDisplay);
                        break;
                    }

                // View all borrowing users
                case 3:
                    {
                        User[] users = [.. library.GetBorrowingUsers()];
                        User? currentUser = ReadMenu("All borrowing users in the library",
                            users, userDisplay);

                        if (currentUser is not null)
                            UserAccount(currentUser);
                        break;
                    }

                case 4:
                default:
                    return;
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

    internal static int ReadMenu(string prompt, string[] options, string? optionPrompt = null, bool newline = true)
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
                        return opt - 1;
                    }
                    break;

                default:
                    if (char.IsDigit(key.KeyChar))
                    {
                        buffer += key.KeyChar;
                    }
                    break;
            }

            //string? line = Console.ReadLine();
            //bool inputSuccess = int.TryParse(line, out int opt);

        }
    }
}
