namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            // •	If there are any validation errors for the book entity(such as invalid name, genre, price, pages or published date),
            // do not import any part of the entity and append an error message to the method output.
            //NOTE: Date will be in format "MM/dd/yyyy", do not forget to use CultureInfo.InvariantCulture

            var output = new StringBuilder();
            var xmlSerializer = new XmlSerializer(
                typeof(BookInputModel[]),
                new XmlRootAttribute("Books"));
            var books = (BookInputModel[])xmlSerializer.Deserialize(
                                new StringReader(xmlString));


            foreach (var xmlBook in books)
            {
                if (!IsValid(xmlBook))
                {
                    output.AppendLine("Invalid data!");
                    continue;
                }
                bool publishedDate = DateTime.TryParseExact(
                    xmlBook.PublishedOn, "MM/dd/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);

                var book = new Book
                {

                    Name = xmlBook.Name,
                   // Genre = xmlBook.Genre.Value,
                    Genre = (Genre)xmlBook.Genre,
                    Price = xmlBook.Price,
                    Pages = xmlBook.Pages,
                    PublishedOn = date,
                };
                context.Books.Add(book);
              context.SaveChanges();
                output.AppendLine($"Successfully imported book {xmlBook.Name} for {xmlBook.Price}.");
            }
           // context.SaveChanges();

            return output.ToString().TrimEnd();
            
        }



        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            // •	If any validation errors occur(such as invalid first name, last name, email or phone),
            // do not import any part of the entity and append an error message to the method output.
            //•	If an email exists, do not import the author and append and error message.
            //•	If a book does not exist in the database, do not append an error message and continue with the next book.
            //•	If an author have zero books(all books are invalid) do not import the author and append an error message to the method output.

            var output = new StringBuilder();
            var authors = JsonConvert
                .DeserializeObject<IEnumerable<AuthorImportModel>>(jsonString);

            foreach (var jsonAuthor in authors)
            {
                if (!IsValid(jsonAuthor))
                {
                    output.AppendLine("Invalid data!");
                    continue;
                }

                var email = context.Authors.FirstOrDefault(x => x.Email == jsonAuthor.Email);
                if (email != null)
                {
                    output.AppendLine("Invalid data!");
                    continue;
                }
                var author = new Author
                {
                    FirstName = jsonAuthor.FirstName,
                    LastName = jsonAuthor.LastName,
                    Phone = jsonAuthor.Phone,
                    Email = jsonAuthor.Email,
                };

                //foreach (var jsonTag in jsonGame.Tags)
                //{
                //    var tag = context.Tags.FirstOrDefault(x => x.Name == jsonTag)
                //                ?? new Tag { Name = jsonTag };
                //    game.GameTags.Add(new GameTag { Tag = tag });
                //}

                //context.Games.Add(game);
                //context.SaveChanges();
                //output.AppendLine($"Added {jsonGame.Name} ({jsonGame.Genre}) with {jsonGame.Tags.Count()} tags");

                foreach (var jsonBook in jsonAuthor.Books)
                {


                    var book = context.Books.Find(jsonBook.Id);
                    if (book == null)
                    {
                        continue;
                    }

                    author.AuthorsBooks.Add(new AuthorBook { Book = book, Author = author });
                }

                if (author.AuthorsBooks.Count == 0)
                {
                    output.AppendLine("Invalid data!");
                    continue;

                }

                context.Authors.Add(author);
                output.AppendLine($"Successfully imported author - {jsonAuthor.FirstName} {jsonAuthor.LastName} with {jsonAuthor.Books.Count()} books.");
            }

            context.SaveChanges();
            return output.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}