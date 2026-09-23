using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Data.Models
{
    //•	Id - integer, Primary Key
    //•	FirstName - text with length[3, 30]. (required)
    //•	LastName - text with length[3, 30]. (required)
    //•	Email - text(required)
    //•	Phone - text (required)
    //•	AuthorsBooks - collection of type AuthorBook
    public class Author
    {
         public Author()
        {
            this.AuthorsBooks = new HashSet<AuthorBook>();
        }
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        [MinLength(3)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(30)]
        [MinLength(3)]
        public string LastName { get; set; }
       
        [Required]
        public string Phone { get; set; }
        
        [Required]
        public string Email { get; set; }
        public ICollection<AuthorBook> AuthorsBooks { get; set; }

    }
   
}
