using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Data.Models
{//        •	AuthorId - integer, Primary Key, Foreign key(required)
 //•	Author -  Author
 //•	BookId - integer, Primary Key, Foreign key(required)
 //•	Book - Book
    public class AuthorBook
    {
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
    }
}
