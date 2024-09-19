using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem
{
    internal class Book(int bookId, string title, string author, DateOnly publicationDate)
    {
        public int BookId { get; } = bookId;
        public string Title { get; } = title;
        public string Author { get; } = author;
        public DateOnly PublicationDate { get; } = publicationDate;
        public int UserId { get; private set; }
        public DateOnly? DueDate { get; private set; }

        public bool IsAvailable
        {
            get => UserId == -1;
        }

        public void AssignUser(int userId, DateOnly dueDate)
        {
            UserId = userId;
            DueDate = dueDate;
        }

        public void RemoveUser()
        {
            UserId = -1;
            DueDate = null;
        }
    }
}
