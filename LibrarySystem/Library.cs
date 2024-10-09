using System.Text.Json.Serialization;

namespace LibrarySystem;

public class Library
{
    /// <summary>
    /// How much in fees are applied per day that a Book is late.
    /// </summary>
    public const double LateFeePerDay = 1.0;

    public List<Book> Books { get; init; } = [];
    public List<User> Users { get; init; } = [];

    [JsonIgnore]
    public List<Book> Copies
    {
        get => [.. Books.GroupBy(b => b.BookId)
            .Select(b => b.First())];
    }

    [JsonInclude] private int nextBookId = 1;
    [JsonInclude] private int nextUserId = 1;
    [JsonInclude] private int nextUniqueBookId = 1;

    /// <summary>
    /// Adds a book title to the library.
    /// </summary>
    /// <param name="title">The title of the new book.</param>
    /// <param name="author">The author of the new book.</param>
    /// <param name="publicationDate">The publication date of the new
    /// book.</param>
    /// <param name="copies">How many copies of this book should be added to the
    /// library.</param>
    public void AddBook(string title, string author, DateOnly publicationDate,
        int copies = 1)
    {
        // Copies of the same book will share an id so they can be identified as
        // copies.
        int bookId = nextBookId++;
        for (int i = 0; i < copies; i++)
        {
            Book book = new(nextUniqueBookId++, bookId, title, author,
                publicationDate);
            Books.Add(book);
        }
    }

    /// <summary>
    /// Add a user to the library system.
    /// </summary>
    /// <param name="name">The new user's name.</param>
    /// <param name="email">The new user's email address.</param>
    public void AddUser(string name, string email)
    {
        // Assign a new user id and increment next one.
        int userId = nextUserId++;
        User user = new(userId, name, email);
        Users.Add(user);
    }

    /// <summary>
    /// Search for a book by its title.
    /// </summary>
    /// <param name="title">Title value to search for.</param>
    /// <returns>An enumerable containing a list of matching books.</returns>
    public IEnumerable<Book> SearchByTitle(string title)
    {
        return Books
            .GroupBy(b => b.BookId)
            .Select(b => b.First())
            .Where(b => b.Title.Contains(title,
                StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Search for a book by its author.
    /// </summary>
    /// <param name="author">Author's name to search for.</param>
    /// <returns>An enumerable containing a list of matching books.</returns>
    public IEnumerable<Book> SearchByAuthor(string author)
    {
        return Books
            .GroupBy(b => b.BookId)
            .Select(b => b.First())
            .Where(b => b.Author.Contains(author,
                StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Search for a book by keyword, will search both its title or author. Will
    /// match based on character order rather than full words.
    /// </summary>
    /// <param name="keyword">Keyword to search for.</param>
    /// <returns>An enumerable containing a list of matching books.</returns>
    public IEnumerable<Book> SearchByKeyword(string keywords)
    {
        return Books
            .GroupBy(b => b.BookId)
            .Select(b => b.First())
            .Where(b =>
                Utils.FuzzySearchContains($"{b.Title}, {b.Author}", keywords))
            .OrderByDescending(b =>
                Utils.FuzzySearchOrder($"{b.Title}, {b.Author}", keywords));
    }

    // public IEnumerable<Book> SearchByKeyword(string keyword)
    // {
    //     return Books
    //         .GroupBy(b => b.BookId)
    //         .Select(b => b.First())
    //         .Where(b => b.Title.Contains(keyword,
    //             StringComparison.CurrentCultureIgnoreCase)
    //             || b.Author.Contains(keyword,
    //                 StringComparison.CurrentCultureIgnoreCase));
    // }

    /// <summary>
    /// Find a User from their UserId number.
    /// </summary>
    /// <param name="id">The userId to find the user.</param>
    /// <returns>The matching user, or null if the user was not found.</returns>
    public User? FindUserById(int id)
    {
        return Users
            .Where(u => u.UserId == id)
            .FirstOrDefault(defaultValue: null);
    }

    /// <summary>
    /// Find a Book from its BookId number.
    /// </summary>
    /// <param name="id">The BookId to find the book.</param>
    /// <returns>The matching book, or null if the book was not found.</returns>
    public Book? FindBookById(int id)
    {
        return Books
            .Where(b => b.BookId == id)
            .FirstOrDefault(defaultValue: null);
    }

    /// <summary>
    /// Find a Book from its UniqueId number.
    /// </summary>
    /// <param name="id">The UniqueId to find the book.</param>
    /// <returns>The matching book, or null if the book was not found.</returns>
    public Book? FindBookByUnqiueId(int id)
    {
        return Books
            .Where(b => b.UniqueId == id)
            .FirstOrDefault(defaultValue: null);
    }

    /// <summary>
    /// Get all available copies of a book.
    /// </summary>
    /// <param name="bookId">The id of the book to find copies of.</param>
    /// <returns>An enumerable containing available copies of the same
    /// title.</returns>
    public IEnumerable<Book> GetAvailableCopies(int bookId)
    {
        return Books.Where(b => b.BookId == bookId && b.IsAvailable);
    }

    /// <summary>
    /// Get all available copies of a book.
    /// </summary>
    /// <param name="book">The book to find other available copies of.</param>
    /// <returns>An enumerable containing available copies of the same
    /// title.</returns>
    public IEnumerable<Book> GetAvailableCopies(Book book)
    {
        return GetAvailableCopies(book.BookId);
    }

    /// <summary>
    /// Get the number of available copies of a book.
    /// </summary>
    /// <param name="bookId">The id of the book to find number of
    /// copies.</param>
    /// <returns>The number of available copies of the book for
    /// borrowing.</returns>
    public int GetNumberOfAvailableCopies(int bookId)
    {
        return GetAvailableCopies(bookId).Count();
    }

    /// <summary>
    /// Get the number of available copies of a book.
    /// </summary>
    /// <param name="book">The book to find number of copies.</param>
    /// <returns>The number of available copies of the book for
    /// borrowing.</returns>
    public int GetNumberOfAvailableCopies(Book book)
    {
        return GetAvailableCopies(book).Count();
    }

    /// <summary>
    /// Get a list of all books that are currently borrowed.
    /// </summary>
    /// <returns>An enumerable containing every book in the library that is
    /// currently borrowed.</returns>
    public IEnumerable<Book> GetBorrowedBooks() =>
        Books.Where(b => !b.IsAvailable);

    /// <summary>
    /// Get a list of all user who are currently borrowing books.
    /// </summary>
    /// <returns>An enumerable containing all users who are currently borrowing
    /// books.</returns>
    public IEnumerable<User> GetBorrowingUsers() =>
        Users.Where(u => u.Books.Count > 0);


}
