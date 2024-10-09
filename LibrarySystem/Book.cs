using System.Text.Json.Serialization;

namespace LibrarySystem;

public class Book(int uniqueId, int bookId, string title, string author,
    DateOnly publicationDate)
{
    public int BookId { get; init; } = bookId;
    public string Title { get; init; } = title;
    public string Author { get; init; } = author;
    public DateOnly PublicationDate { get; init; } = publicationDate;

    [JsonInclude] public int UserId { get; private set; } = -1;

    [JsonInclude] public DateOnly? DueDate { get; private set; } = null;
    public int UniqueId { get; init; } = uniqueId;

    /// <summary>
    /// Returns whether the book is available, based on if the UserId is -1.
    /// </summary>
    [JsonIgnore] public bool IsAvailable { get => UserId == -1; }

    /// <summary>
    /// Assign a User to a Book, will set the Book's UserId and DueDate
    /// properties.
    /// </summary>
    /// <param name="userId">The userId to assign the Book to.</param>
    /// <param name="dueDate">The new due date, when the book should be
    /// returned.</param>
    public void AssignUser(int userId, DateOnly dueDate)
    {
        UserId = userId;
        DueDate = dueDate;
    }

    /// <summary>
    /// Assign a User to a Book, will set the Book's UserId and DueDate
    /// properties.
    /// </summary>
    /// <param name="user">To User to assign the Book to.</param>
    /// <param name="dueDate">The new due date, when the book should be
    /// returned.</param>
    public void AssignUser(User user, DateOnly dueDate)
    {
        UserId = user.UserId;
        DueDate = dueDate;
    }

    /// <summary>
    /// Removes the User information from this book, should be called when
    /// returning Book.
    /// </summary>
    public void RemoveUser()
    {
        UserId = -1;
        DueDate = null;
    }
}
