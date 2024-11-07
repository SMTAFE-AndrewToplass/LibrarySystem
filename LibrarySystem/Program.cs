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
            return;
        }

        string mainMenuPrompt = "Welcome to the Library system\n";
        mainMenuPrompt += "\n  To select and option, use the arrow keys or type and";
        mainMenuPrompt += "\n  press enter, and press ESC to exit the menu.";

        while (!Utils.Menu(prompt: mainMenuPrompt, options:
        [
            ("Create new user account",
                () => CreateUser()),

            ("Login as user account",
                () => SelectUser()),

            ("Login as admin user",
                () => AdminAccount()),

            ("Exit program",
                null),
        ]));
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

        foreach (User user in library.Users)
        {
            Console.WriteLine($"new User({user.UserId}, \"{user.Name}\", \"{user.Email}\", {user.FeesOwed}),");
        }

        foreach (Book book in library.Books)
        {
            DateOnly d = book.PublicationDate;
            Console.WriteLine($"new Book({book.UniqueId}, {book.BookId}, \"{book.Title}\", \"{book.Author}\", new({d.Year}, {d.Month}, {d.Day})),");
        }
    }

    internal static void CreateUser()
    {
        Console.Clear();
        Console.WriteLine("Create a new user");
        string name = Utils.ReadString("Please enter your name:");
        string email = Utils.ReadString("Please enter your email address:");
        library.AddUser(name, email);
        Console.WriteLine($"User {name} added successfully.");
    }

    internal static void SelectUser(IEnumerable<User>? source = null,
        string prompt = "Please select from the list of users:")
    {
        // default user list is library.Users
        source ??= library.Users;
        User[] users = [.. source];
        User? currentUser = Utils.ReadMenu(prompt,
            users, u => $"{u.UserId}, {u.Name} ({u.Email})");

        if (currentUser is not null)
            UserAccount(currentUser);
    }

    internal static void UserAccount(User user)
    {
        while (true)
        {
            int res = Utils.ReadMenu(
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

                        Utils.Prompt(prompt);
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
                                DateOnly dueDate = (DateOnly)user.Books[0].DueDate!;
                                Utils.Prompt($"Books borrowed successfully, they are due on {dueDate}.");
                            }
                            catch (Exception e)
                            {
                                Utils.Prompt(
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

                            res = Utils.ReadMenu(prompt, ["Yes", "No"], newline: true);
                            if (res == 0)
                            {
                                try
                                {
                                    user.ReturnBooks();
                                    Utils.Prompt("Books returned successfully");
                                }
                                catch (Exception e)
                                {
                                    Utils.Prompt(
                                        "Unable to return books."
                                        + $"\n  Error: {e.Message} \n");
                                }
                            }
                            break;
                        }
                        else
                        {
                            prompt = "You are not currently borrowing any books.\n";
                            Utils.Prompt(prompt);
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
                            prompt = $"You currently owe ${user.FeesOwed:.00}";
                            prompt += $" in late fees.\n\n";
                            prompt += "Would you like to pay your outstanding fees?";

                            res = Utils.ReadMenu(prompt, ["Yes", "No"], newline: true);
                            if (res == 0)
                            {
                                user.PayFee(fees);
                                Utils.Prompt("Payment successful.");
                            }

                            break;
                        }
                        else
                        {
                            prompt = "You do have any outstanding fees.\n";
                            Utils.Prompt(prompt);
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
                    string bookInfo = $"{book.Title}, {book.Author}";
                    SearchIndex[] indices = Utils.FuzzySearchDetailed(
                        bookInfo, query);

                    // Additional information, not included in search.
                    bookInfo += $", published: {book.PublicationDate}";
                    bookInfo += $", available: {library.GetNumberOfAvailableCopies(book)} ";

                    if (selected == i)
                        Console.BackgroundColor = ConsoleColor.DarkGray;

                    Console.Write(selection.Contains(book) ? " [x] " : " [ ] ");

                    Utils.WriteHighlighted(bookInfo, indices,
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
                    string bookInfo = $"{book.Title}, {book.Author}";

                    // Additional information, not included in search.
                    bookInfo += $", published: {book.PublicationDate}";
                    bookInfo += $", available: {library.GetNumberOfAvailableCopies(book)} ";

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

        int result = Utils.ReadMenu(prompt, [
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

    internal static void SelectBook(IEnumerable<Book>? source = null,
        string prompt = "Please select from the list of books:")
    {
        // default user list is library.Users
        source ??= library.AvailableCopies;
        Book[] users = [.. source];
        Book? currentBook = Utils.ReadMenu(prompt,
            users, book =>
            {
                User? user = library.FindUserById(book.UserId);
                string userInfo = "";
                if (user is not null)
                {
                    userInfo = $", borrowed by {user.Name} ({user.UserId})";
                }
                return $"{book.UniqueId}: {book.Title}, {book.Author}{userInfo}";
            });

        if (currentBook is not null)
            BookInfo(currentBook);
    }

    public static void BookInfo(Book book)
    {
        string bookInfo = $"[Book information]\n";
        bookInfo += $"  Title: {book.Title}\n";
        bookInfo += $"  Author: {book.Author}\n";
        bookInfo += $"  Publication Date: {book.PublicationDate}\n";
        bookInfo += $"  BookId: {book.BookId}\n";
        bookInfo += $"  UniqueId: {book.UniqueId}\n";

        if (book.UserId >= 0)
        {
            User? user = library.FindUserById(book.UserId);
            if (user is not null && book.DueDate is not null)
            {
                bookInfo += $"  Borrowed by: {user.Name} ({user.UserId})\n";
                bookInfo += $"  Due date: {book.DueDate}\n";
            }
        }
        Utils.Prompt(bookInfo);
    }

    internal static void AdminAccount()
    {
        bool exit = false;
        while (!exit)
        {
            exit = Utils.Menu(
                "[Admin mode]:",
                [
                    ("View all books",
                        () => SelectBook(library.Books,
                            prompt: "All books in the library")),

                    ("View all users",
                        () => SelectUser(prompt: "All users")),

                    ("View all borrowed books",
                        () => SelectBook(library.GetBorrowedBooks(),
                            prompt: "All borrowed books in the library")),

                    ("View all borrowing users",
                        () => SelectUser(library.GetBorrowingUsers(),
                            prompt: "All borrowing users")),

                    ("Logout", null),
                ]
            );
        }
    }
}
