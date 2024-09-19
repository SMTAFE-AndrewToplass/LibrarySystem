using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{
    internal class User(int userId, string name, string email, double feesOwed = 0)
    {
        protected const int maxNumberOfBooks = 3;
        protected const int borrowDays = 5;

        public int UserId { get; } = userId;
        public string Name { get; } = name;
        public string Email { get; } = email;
        public List<Book> Books { get; } = [];
        public double FeesOwed { get; private set; } = feesOwed;

        public void BorrowBooks(IEnumerable<Book> books)
        {
            if (Books.Count > 0)
            {
                throw new Exception($"User {Name} must return their borrowed books before borrowing again.");
            }

            // Get todays date for 
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            if (books.Count() > maxNumberOfBooks)
            {
                throw new ArgumentException($"You can only borrow {maxNumberOfBooks} books at a time.");
            }

            foreach (Book book in books)
            {
                if (book.IsAvailable)
                {
                    DateOnly dueDate = today.AddDays(borrowDays);
                    book.AssignUser(UserId, dueDate);
                    Books.Add(book);
                }
                else
                {
                    throw new ArgumentException($"Cannot borrow book {book.Title}, as it is not available.");
                }
            }
        }

        public void ReturnBooks(IEnumerable<Book> books)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            foreach (Book book in books)
            {
                if (book.UserId == UserId && book.DueDate is not null)
                {
                    DateOnly dueDate = (DateOnly)book.DueDate;
                    int daysLate = Math.Max(today.DayNumber - dueDate.DayNumber, 0);
                    double fees = Library.lateFeePerDay * daysLate;
                    FeesOwed += fees;

                    book.RemoveUser();
                    Books.Remove(book);
                }
                else
                {
                    throw new ArgumentException($"User {Name} has not borrowed book {book.Title}.");
                }
            }
        }
    }

    //internal class VIPUser(int userId, string name, string email, double feesOwed = 0) : User(userId, name,email, feesOwed)
    //{
    //    protected new const int maxNumberOfBooks = 5;
    //}
}
