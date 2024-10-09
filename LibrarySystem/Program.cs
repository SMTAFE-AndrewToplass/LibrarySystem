using System.Text.Json;

namespace LibrarySystem;

internal static class Program
{
    internal static Library library = new();
    internal static string libraryPath = Path.Combine(Environment.CurrentDirectory, "library.json");
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

        InteractiveSearch(library.SearchByKeyword);

        // while (true)
        // {
        //     int res = ReadMenu(
        //         prompt: "Welcome to the Library system.",
        //         options: [
        //             "Create new user account",
        //             "Login as user account",
        //             "Login as admin user",
        //             "Exit program"
        //         ]
        //     );

        //     switch (res)
        //     {
        //         case 0:
        //             CreateUser();
        //             break;
        //         case 1:
        //             SelectUser();
        //             break;
        //         case 2:
        //             AdminAccount();
        //             break;
        //         case 3:
        //             return;
        //     }
        // }

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
        Console.Clear();
        Console.WriteLine("Please select from the list of users:");
        foreach (var user in library.Users)
        {
            Console.WriteLine($"{user.UserId}, {user.Name} ({user.Email})");
        }
        int result = ReadInt();
        User? currentUser = library.FindUserById(result);
        if (currentUser is not null)
        {
            UserAccount(currentUser);
        }
    }

    internal static void UserAccount(User user)
    {
        Console.WriteLine($"Welcome, {user.Name}");
        while (true)
        {
            int res = ReadMenu(
                prompt: "What would you like to do today:",
                options: [
                    "View my borrowed books",
                    "Borrow some new books",
                    "Return borrowed books",
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
                        foreach (Book book in books)
                        {
                            // Print each book's information.
                            Console.WriteLine(
                                $"{book.BookId}: {book.Author}, {book.Title}");
                        }
                        break;
                    }

                case 1:
                    {
                        IEnumerable<Book> books = SelectBooks();
                        break;
                    }

                case 4:
                    return;
            }
        }
    }

    internal static IEnumerable<Book> InteractiveSearch(
        Func<string, IEnumerable<Book>> searchMethod)
    {
        List<Book> books = [];

        Console.Clear();
        Console.WriteLine("Enter some characters to begin searching.");

        string query = "";

        // Cursor position, relative to left-most column.
        int left = 0;
        int scroll = 0;

        while (true)
        {
            int maxHeight = Console.BufferHeight - 2;
            int bottom = Console.WindowHeight - 1;
            Console.CursorTop = bottom;
            Console.Write(query);
            Console.CursorLeft = left;
            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.Key)
            {
                // Exit search mode.
                case ConsoleKey.Escape:
                    if (query.Length == 0)
                        return [];
                    query = "";
                    left = 0;
                    break;

                // Delete char infront of cursor.
                case ConsoleKey.Delete:
                    if (left < query.Length)
                    {
                        query = query.Remove(left, 1);
                        // Delete, clear next char, then go back one char.
                        Console.Write(" \b");
                    }
                    break;

                // Delete char behind cursor.
                case ConsoleKey.Backspace:
                    if (left > 0)
                    {
                        query = query.Remove(left - 1, 1);
                        // Backspace. Go back one char, erase char with space
                        // (advances cursor forward), then go back again.
                        Console.Write("\b \b");
                        left--;
                    }

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
                    if (scroll > 0)
                        scroll--;
                    break;

                case ConsoleKey.DownArrow:
                    if (scroll > 0)
                        scroll--;
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
                        query = query.Insert(left, key.KeyChar.ToString());
                        left++;
                    }
                    break;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Interactive Search: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Up/Down: Scroll, Enter: Select, Esc: Clear/Exit");
            Utils.ResetForeground();

            if (query.Length > 0)
            {
                List<Book> res = [.. searchMethod(query)];
                for (int i = 0; i < res.Count && i < maxHeight; i++)
                {
                    Book book = res[i];
                    string bookInfo = $"{book.Title}, {book.Author}";
                    SearchIndex[] indices = Utils.FuzzySearchDetailed(bookInfo, query);
                    WriteHighlighted(bookInfo, indices);
                }
            }
            else
            {
                List<Book> res = library.Copies;
                for (int i = 0; i < res.Count && i < maxHeight; i++)
                {
                    Book book = res[i];
                    string bookInfo = $"{book.Title}, {book.Author}";
                    Console.WriteLine(bookInfo);
                }
            }


        }

        // return books;
    }

    public static void WriteHighlighted(string value, SearchIndex[] indices)
    {
        static void highlight(string v)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(v);
            Utils.ResetForeground();
        }

        if (indices.Length < 1)
        {
            Console.WriteLine(value);
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
                Console.WriteLine(value[i..^0]);
                i = value.Length;
            }
        }

        // int i = 0;
        // for (; i < indices.Length - 1; i += 1)
        // {
        //     SearchIndex index = indices[i];
        //     SearchIndex next = indices[i + 1];
        //     if (i == 0)
        //     {
        //         Console.Write(value[..index.Start]);
        //     }
        //     highlight(value[index.Start..index.End]);
        //     Console.Write(value[index.End..next.Start]);
        // }
        // SearchIndex last = indices[i];
        // highlight(value[last.Start..last.End]);
        // Console.WriteLine(value[last.End..^0]);
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

    internal static int ReadMenu(string[] options, string? prompt = null,
        bool clear = false)
    {
        if (clear)
            Console.Clear();
        while (true)
        {
            // Print prompt.
            if (!string.IsNullOrEmpty(prompt))
            {
                Console.WriteLine(prompt);
            }
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}: {options[i]}");
            }
            Console.Write($"Enter option (1-{options.Length}): ");

            bool inputSuccess = int.TryParse(Console.ReadLine(), out int opt);
            if (inputSuccess && opt > 0 && opt <= options.Length)
            {
                return opt - 1;
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Invalid input, please try again.\n");
            }
        }
    }
}
