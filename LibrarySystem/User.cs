using System.Text.Json.Serialization;

namespace LibrarySystem;

public class User(int userId, string name, string email, double feesOwed = 0)
{
    /// <summary>
    /// How many books can a User borrow at once.
    /// </summary>
    [JsonIgnore] public virtual int MaxNumberOfBooks { get => 3; }

    /// <summary>
    /// How many days a Book can be borrowed for before late fees.
    /// </summary>
    [JsonIgnore] public virtual int BorrowDays { get => 5; }

    public int UserId { get; init; } = userId;
    public string Name { get; init; } = name;
    public string Email { get; init; } = email;
    [JsonIgnore] public List<Book> Books { get; init; } = [];
    [JsonInclude] public double FeesOwed { get; private set; } = feesOwed;

    public List<int> UniqueBookIds { get; init; } = [];

    /// <summary>
    /// Borrow a list of Books, up to User.MaxNumberOfBooks at a time.
    /// </summary>
    /// <param name="books">A list of Books to borrow.</param>
    /// <exception cref="Exception">Thrown if User did not return their Books or
    /// did not pay their fees before borrowing again.</exception>
    /// <exception cref="ArgumentException">Thrown if there are too many Books
    /// at once or a Book is not available.</exception>
    public void BorrowBooks(params Book[] books)
    {
        if (FeesOwed > 0.0)
        {
            // User must pay any fees they owe before borrwing again.
            throw new Exception(
                $"User {Name} must pay their late fees before borrowing again.");
        }

        if (Books.Count > 0)
        {
            // User must return their existing Books before borrowing again.
            throw new Exception(
                $"User {Name} must return their borrowed books before borrowing again.");
        }

        // Get todays date to calculate new due date.
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        if (books.Length > MaxNumberOfBooks)
        {
            throw new ArgumentException(
                $"You can only borrow {MaxNumberOfBooks} book(s) at a time.");
        }

        foreach (Book book in books)
        {
            // Check if the Book is available before borrowing it.
            if (book.IsAvailable)
            {
                // Set the due date to today + how many days the Book can be
                // borrowed for.
                DateOnly dueDate = today.AddDays(BorrowDays);

                // Add the User's information to the Book, then add it to the
                // User's Book list.
                book.AssignUser(UserId, dueDate);
                Books.Add(book);
                UniqueBookIds.Add(book.UniqueId);
            }
            else
            {
                throw new ArgumentException(
                    $"Cannot borrow book {book.Title}, as it is not available.");
            }
        }
    }

    /// <summary>
    /// Borrow a list of Books, up to User.MaxNumberOfBooks at a time.
    /// </summary>
    /// <param name="books">A list of Books to borrow.</param>
    /// <exception cref="Exception">Thrown if User did not return their Books or
    /// did not pay their fees before borrowing again.</exception>
    /// <exception cref="ArgumentException">Thrown if there are too many Books
    /// at once or a Book is not available.</exception>
    public void BorrowBooks(IEnumerable<Book> books) => BorrowBooks([.. books]);

    /// <summary>
    /// Return all books the user is currently borrowing.
    /// </summary>
    public void ReturnBooks()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        foreach (Book book in Books)
        {
            if (book.UserId == UserId && book.DueDate is not null)
            {
                DateOnly dueDate = (DateOnly)book.DueDate;

                // Get the number of days between today and the book's due date,
                // limited to 0 if the due date is in the future.
                int daysLate = Math.Max(today.DayNumber - dueDate.DayNumber, 0);

                // Apply late fees for returning the Book late (no fees are
                // applied if daysLate is 0).
                double fees = Library.LateFeePerDay * daysLate;
                FeesOwed += fees;

                // Remove the User information from the Book and from the User's
                // Book list.
                book.RemoveUser();
                Books.Remove(book);
                UniqueBookIds.Remove(book.UniqueId);
            }
        }
    }

    /// <summary>
    /// Make a payment for their account
    /// </summary>
    /// <param name="amount"></param>
    public void PayFee(double amount)
    {
        FeesOwed -= amount;
    }
}

// internal class VIPUser(int userId, string name, string email,
//     double feesOwed = 0) : User(userId, name, email, feesOwed)
// {
//    /// <summary>
//    /// How many books can a User borrow at once.
//    /// </summary>
//    public override int MaxNumberOfBooks { get => 5; }

//    /// <summary>
//    /// How many days a Book can be borrowed for before late fees.
//    /// </summary>
//    public override int BorrowDays { get => 10; }
// }
