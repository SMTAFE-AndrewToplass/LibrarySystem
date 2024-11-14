using LibrarySystem;
using NUnit.Framework.Internal;

namespace LibrarySystemTests;

public class Tests
{
    Library library;
    static readonly Book[] testBooks =
        [
            new Book(1, 1, "The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1)),
            new Book(2, 2, "To Kill a Mockingbird", "Harper Lee", new(1960, 1, 1)),
            new Book(3, 3, "1984", "George Orwell", new(1949, 1, 1)),
            new Book(4, 4, "Pride and Prejudice", "Jane Austen", new(1813, 1, 1)),
            new Book(5, 5, "The Catcher in the Rye", "J.D. Salinger", new(1951, 1, 1)),
            new Book(6, 6, "The Lord of the Rings", "J.R.R. Tolkien", new(1954, 1, 1)),
            new Book(7, 7, "The Kite Runner", "Khaled Hosseini", new(2003, 1, 1)),
            new Book(8, 8, "The Hunger Games", "Suzanne Collins", new(2008, 1, 1)),
            new Book(9, 9, "The Da Vinci Code", "Dan Brown", new(2003, 1, 1)),
            new Book(10, 10, "Gone with the Wind", "Margaret Mitchell", new(1936, 1, 1)),
            new Book(11, 11, "The Shining", "Stephen King", new(1977, 1, 1)),
            new Book(12, 12, "The Hobbit", "J.R.R. Tolkien", new(1937, 1, 1)),
        ];

    [SetUp]
    public void Setup()
    {
        library = new Library();
    }

    private void AddTestBooks()
    {
        foreach (Book book in testBooks)
        {
            library.AddBook(book);
        }
    }

    // Used to compare books only by title, author, and pub.date.
    class BookEqualityComparer : IEqualityComparer<Book>
    {
        public bool Equals(Book? x, Book? y) =>
            x is not null && y is not null && GetHashCode(x) == GetHashCode(y);
        public int GetHashCode(Book book) =>
            HashCode.Combine(book.Title, book.Author, book.PublicationDate);
    }

    // Used to compare books only by title, author, and pub.date.
    class UserEqualityComparer : IEqualityComparer<User>
    {
        public bool Equals(User? x, User? y) =>
            x is not null && y is not null && GetHashCode(x) == GetHashCode(y);
        public int GetHashCode(User user) =>
            HashCode.Combine(user.Name, user.Email, user.FeesOwed);
    }

    [Test]
    [TestCase("The Great Gatsby", "F. Scott Fitzgerald", 1925, 1)]
    [TestCase("To Kill a Mockingbird", "Harper Lee", 1960, 5)]
    [TestCase("1984", "George Orwell", 1949, 0)]
    [TestCase("Pride and Prejudice", "Jane Austen", 1813, -1)]
    public void AddBook(string title, string author, int publicationYear, int copies)
    {
        library.AddBook(title, author, new DateOnly(publicationYear, 1, 1), copies);

        if (copies > 0)
        {
            // Make sure that at least one book was added.
            Assert.That(library.Books, Has.Count.GreaterThan(0));
        }

        // Get the book that was just added.
        Book? book = library.Books.LastOrDefault();

        BookEqualityComparer bookEquals = new();

        // Count the number of copies that were added.
        int actualCopies = library.Books.Aggregate(0, (a, b) => bookEquals.Equals(book, b) ? a + 1 : a);
        // How many copies should've been added, equal to copies unless negative.
        int expectedCopies = copies > 0 ? copies : 0;

        // Ensure that the correct number of copies were added.
        Assert.That(actualCopies, Is.EqualTo(expectedCopies));
    }

    [Test]
    [TestCase("Emily Johnson", "emily.johnson@example.com")]
    [TestCase("Michael Lee", "michael.lee@example.com")]
    [TestCase("Sophia Patel", "sophia.patel@example.com")]
    public void AddUser(string name, string email)
    {
        UserEqualityComparer userEquals = new();
        User testUser = new(0, name, email);
        library.AddUser(name, email);
        Assert.That(library.Users.Contains(testUser, userEquals));
    }

    [Test]
    public void BorrowBook()
    {
        library.AddUser("Emily Johnson", "emily.johnson@example.com");
        User user = library.Users.Last();

        library.AddBook("The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1));
        library.AddBook("To Kill a Mockingbird", "Harper Lee", new(1960, 1, 1));
        Book book1 = library.Books[0];
        Book book2 = library.Books[1];

        user.BorrowBooks(book1, book2);

        Assert.That(user.Books, Does.Contain(book1));
        Assert.That(user.Books, Does.Contain(book2));
    }

    [Test]
    public void BorrowBookTooManyBooks()
    {
        library.AddUser("Emily Johnson", "emily.johnson@example.com");
        User user = library.Users.Last();

        library.AddBook("The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1));
        library.AddBook("To Kill a Mockingbird", "Harper Lee", new(1960, 1, 1));
        library.AddBook("1984", "George Orwell", new(1949, 1, 1));
        library.AddBook("Pride and Prejudice", "Jane Austen", new(1813, 1, 1));

        Book[] books = [.. library.Books];

        Assert.That(() => { user.BorrowBooks(books); }, Throws.Exception.TypeOf<ArgumentException>());
    }

    [Test]
    public void BorrowBookLateFeesDue()
    {
        library.AddUser(new(-1, "Emily Johnson", "emily.johnson@example.com", 10.00));
        User user = library.Users.Last();
        
        library.AddBook("The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1));
        Book book = library.Books.Last();

        Assert.That(() => { user.BorrowBooks(book); }, Throws.Exception.TypeOf<Exception>());
    }

    [Test]
    public void BorrowBookAlreadyBorrowed()
    {
        library.AddUser(new(-1, "Emily Johnson", "emily.johnson@example.com"));
        library.AddUser(new(-1, "Michael Lee", "michael.lee@example.com"));
        User user1 = library.Users[0];
        User user2 = library.Users[1];

        library.AddBook("The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1));
        Book book = library.Books.Last();

        user1.BorrowBooks(book);

        Assert.That(() => { user2.BorrowBooks(book); }, Throws.Exception.TypeOf<ArgumentException>());
    }

    [Test]
    public void BorrowBookAlreadyBorrowing()
    {
        library.AddUser(new(-1, "Emily Johnson", "emily.johnson@example.com"));
        User user = library.Users.Last();

        library.AddBook("The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1));
        Book book = library.Books.Last();

        user.BorrowBooks(book);

        // Attempt to borrow book again.
        Assert.That(() => { user.BorrowBooks(book); }, Throws.Exception.TypeOf<Exception>());
    }

    [Test]
    public void ReturnBook()
    {
        library.AddUser(new(-1, "Emily Johnson", "emily.johnson@example.com"));
        User user = library.Users.Last();

        library.AddBook("The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1));
        Book book = library.Books.Last();

        user.BorrowBooks(book);

        user.ReturnBooks();

        Assert.That(user.Books, Has.Count.EqualTo(0));
    }

    [Test]
    public void ReturnBookLate()
    {
        library.AddUser(new(-1, "Emily Johnson", "emily.johnson@example.com"));
        User user = library.Users.Last();

        library.AddBook("The Great Gatsby", "F. Scott Fitzgerald", new(1925, 1, 1));
        Book book = library.Books.Last();

        user.BorrowBooks(book);

        // Change the due date of the book to be in the past.
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly dueDate = today.AddDays(-(user.BorrowDays + 10));
        book.AssignUser(user, dueDate);

        user.ReturnBooks();

        Assert.Multiple(() =>
        {
            // The book should be returned.
            Assert.That(user.Books, Has.Count.EqualTo(0));
            // Late fees should be added.
            Assert.That(user.FeesOwed, Is.GreaterThan(0));
        });
    }

    [Test]
    [TestCase("The Great Gatsby", 1)]
    [TestCase("The", 9)]
    [TestCase("does not exist", 0)]
    public void SearchForBookByTitle(string title, int expectedResults)
    {
        AddTestBooks();
        IEnumerable<Book> books = library.SearchByTitle(title);
        Assert.That(books.Count(), Is.EqualTo(expectedResults));
    }

    [Test]
    [TestCase("J.R.R. Tolkien", 2)]
    [TestCase("F. Scott Fitzgerald", 1)]
    [TestCase("does not exist", 0)]
    public void SearchForBookByAuthor(string author, int expectedResults)
    {
        AddTestBooks();
        IEnumerable<Book> books = library.SearchByAuthor(author);
        Assert.That(books.Count(), Is.EqualTo(expectedResults));
    }

    [Test]
    [TestCase("J.R.R.", 2)]
    [TestCase("The great", 1)]
    [TestCase("does not exist", 0)]
    public void SearchForBookByKeyword(string keyword, int expectedResults)
    {
        AddTestBooks();
        IEnumerable<Book> books = library.SearchByKeyword(keyword);
        Assert.That(books.Count(), Is.EqualTo(expectedResults));
    }
}