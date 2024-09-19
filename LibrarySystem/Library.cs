using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{
    internal class Library
    {
        internal const double lateFeePerDay = 1.0;
        internal const int daysBeforeLateFees = 3;

        public List<Book> Books { get; } = [];
        public List<User> Users { get; } = [];

        private int nextBookId = 0;
        private int nextUserId = 0;

        public void AddBook(string title, string author, DateOnly publicationDate, int copies)
        {
            int bookId = nextBookId++;
            for (int i = 0; i < copies; i++)
            {
                Book book = new(bookId, title, author, publicationDate);
                Books.Add(book);
            }
        }

        public void AddUser(string name, string email)
        {
            int userId = nextUserId++;
            User user = new(userId, name, email);
            Users.Add(user);
        }

        public IEnumerable<Book> SearchByTitle(string title)
        {
            return Books.Where(b => b.Title.Contains(title));
        }

        public IEnumerable<Book> SearchByAuthor(string author)
        {
            return Books.Where(b => b.Author.Contains(author));
        }

        public IEnumerable<Book> SearchByKeyword(string keyword)
        {
            return Books.Where(b => b.Title.Contains(keyword) || b.Author.Contains(keyword));
        }

        public User? GetUserById(int id)
        {
            foreach (var user in Users)
            {
                if (user.UserId == id)
                {
                    return user;
                }
            }
            return null;
        }

        public Book? GetBookById(int id)
        {
            foreach (var book in Books)
            {
                if (book.BookId == id)
                {
                    return book;
                }
            }
            return null;
        }

        public IEnumerable<Book> GetAvailableCopies(int bookId)
        {
            return Books.Where(b => b.BookId == bookId && b.IsAvailable);
        }

        public IEnumerable<Book> GetAvailableCopies(Book book)
        {
            return GetAvailableCopies(book.BookId);
        }

        public int GetNumberOfAvailableCopies(int bookId)
        {
            return GetAvailableCopies(bookId).Count();
        }

        public int GetNumberOfAvailableCopies(Book book)
        {
            return GetAvailableCopies(book).Count();
        }

        public IEnumerable<Book> GetBorrowedBooks() => Books.Where(b => !b.IsAvailable);

        public IEnumerable<User> GetBorrowingUsers() => Users.Where(u => u.Books.Count > 0);
    }
}
