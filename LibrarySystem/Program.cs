using System.Linq.Expressions;

namespace LibrarySystem;

internal class Program
{
    internal static Library library = new();

    internal static void Main(string[] args)
    {
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
                    return;
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
                            Console.WriteLine($"{book.BookId}: {book.Author}, {book.Title}");
                        }
                        break;
                    }

                case 1:
                    {
                        break;
                    }

                case 4:
                    return;
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
