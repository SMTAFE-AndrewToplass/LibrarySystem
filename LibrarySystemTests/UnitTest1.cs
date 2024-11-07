using LibrarySystem;
using NUnit.Framework.Internal;

namespace LibrarySystemTests;

public class Tests
{
    Library library;
    Book[] testBooks = [
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

    User[] testUsers =
        [
            new User(1, "Emily Johnson", "emily.johnson@example.com", 0),
            new User(2, "Michael Lee", "michael.lee@example.com", 0),
            new User(3, "Sophia Patel", "sophia.patel@example.com", 0),
            new User(4, "David Hernandez", "david.hernandez@example.com", 10.78),
            new User(5, "Olivia Nguyen", "olivia.nguyen@example.com", 0),
            new User(7, "Jonn Smith", "john.smith@example.com", 0),
        ];

    [SetUp]
    public void Setup()
    {
        library = new Library();


    }

    // Used to compare books only by title, author, and pub.date.
    class BookEqualityComparer : IEqualityComparer<Book>
    {
        public bool Equals(Book? x, Book? y) =>
            x is not null && y is not null && GetHashCode(x) == GetHashCode(y);
        public int GetHashCode(Book book) =>
            HashCode.Combine(book.Title, book.Author, book.PublicationDate);
    }

    [Test]
    public void AddBook()
    {
        foreach (Book book in testBooks)
        {
            library.AddBook(book.Title, book.Author, book.PublicationDate);
        }

        BookEqualityComparer bookEquals = new();
        foreach (Book book in testBooks)
        {
            Assert.That(library.Books.Contains(book, bookEquals));
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(-3)]
    [TestCase(1)]
    [TestCase(3)]
    public void AddBookMultipleCopies(int copies)
    {

    }
}