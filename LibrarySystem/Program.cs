using System.Linq.Expressions;

namespace LibrarySystem;

internal static class Program
{
    internal static Library library = new();

    internal static void Main(string[] args)
    {
        DateOnly now = DateOnly.FromDateTime(DateTime.Now);
        library.AddBook("Hello World", "Andrew Toplass", now, 2);
        library.AddBook("Admission", "Author 2", now);
        library.AddBook("Schadenfreude", "Author 3", now, 5);
        library.AddBook("Firmware", "Author 4", now);
        library.AddBook("Lorem Ipsum", "Someone", now);

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
                            Console.WriteLine($"{book.BookId}: {book.Author}, {book.Title}");
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

    internal static IEnumerable<Book> InteractiveSearch(Func<string, IEnumerable<Book>> searchMethod)
    {
        List<Book> books = [];

        Console.Clear();
        Console.WriteLine("Enter some characters to begin searching.");

        string query = "";
        // int left = 0;
        int bottom = Console.WindowHeight - 2;

        while (true)
        {
            // Console.SetCursorPosition(left, bottom);
            Console.CursorTop = bottom;
            Console.Write(query);
            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    query = query.RemoveLast();
                    // Backspace. Go back one char, erase char with space
                    // (advances cursor forward), then go back again.
                    Console.Write("\b \b");
                    break;

                default:
                    {
                        // Only add printable characters to search query.
                        if (!char.IsControl(key.KeyChar))
                        {
                            query += key.KeyChar;
                        }
                        break;
                    }
            }

            // left = Console.CursorLeft;
            Console.Clear();
            Console.WriteLine("Interactive Search By Title.");

            if (query.Length > 0)
            {
                IEnumerable<Book> res = searchMethod(query);
                foreach (Book book in res)
                {
                    Console.WriteLine("{0}, {1}", book.Title, book.Author);
                }
            }


        }

        // return books;
    }

    internal static string RemoveLast(this string str)
    {
        if (str.Length == 0)
        {
            return str;
        }
        else
        {
            return str.Remove(str.Length - 1);
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

    internal static int ReadMenu(string[] options, string? prompt = null, bool clear = false)
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
