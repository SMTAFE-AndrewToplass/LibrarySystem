using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem;

internal class Book(int bookId, string title, string author, DateOnly publicationDate)
{
    public int BookId { get; } = bookId;
    public string Title { get; } = title;
    public string Author { get; } = author;
    public DateOnly PublicationDate { get; } = publicationDate;
    public int UserId { get; private set; }
    public DateOnly? DueDate { get; private set; }

    /// <summary>
    /// Returns a bool to state whether the book is available, based on if the UserId is -1.
    /// </summary>
    public bool IsAvailable
    {
        get => UserId == -1;
    }

    /// <summary>
    /// Assign a User to a Book, will set the Book's UserId and DueDate properties.
    /// </summary>
    /// <param name="userId">The userId to assign the Book to.</param>
    /// <param name="dueDate">The new due date, when the book should be returned.</param>
    public void AssignUser(int userId, DateOnly dueDate)
    {
        UserId = userId;
        DueDate = dueDate;
    }

    /// <summary>
    /// Removes the User information from this book, should be called when returning Book.
    /// </summary>
    public void RemoveUser()
    {
        UserId = -1;
        DueDate = null;
    }
}
