namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            //            Export Most Craziest Authors
            //Select all authors along with their books.
            //Select their name in format first name + ' ' + last name.
            //For each book select its name and price formatted to the second digit after the decimal point.
            //Order the books by price in descending order. Finally sort all authors by book count descending and then by author full name.



            var data = context.Authors//.ToList()
              .Select(x => new
              {
                  AuthorName = x.FirstName + " " + x.LastName,

                  Books = x.AuthorsBooks.Select(ab => ab.Book)
                  .OrderByDescending(ab => ab.Price)
                  .Select(b => new
                  {
                      BookName = b.Name,
                      BookPrice = b.Price.ToString("F2"),
                  }).ToArray()


              })
              .ToArray()
              .OrderByDescending(x => x.Books.Count())
              .ThenBy(x => x.AuthorName)
              .ToArray();

            return JsonConvert.SerializeObject(data, Formatting.Indented);

        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {

            //Export top 10 oldest books that are published before the given date and are of type science. 
            //    For each book select its name, date (in format "d") and pages.
            //    Sort them by pages in descending order and then by date in descending order.

            //         < Books >
            //< Book Pages = "4881" >
            //   < Name > Sierra Marsh Fern</ Name >
            //      < Date > 03 / 18 / 2016 </ Date >
            //    </ Book >

            var data = context.Books.ToList()
.Where(x => x.PublishedOn < date && (int)x.Genre == 3)
                .Select(x => new BookOutputModel
                {
                    Name = x.Name,
                    PublishedOn = x.PublishedOn.ToString("d", CultureInfo.InvariantCulture),
                    Pages = x.Pages
                })
                .OrderByDescending(x => x.Pages)
                .ThenByDescending(x => x.PublishedOn)
                .Take(10)
                .ToArray();
              
            

            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(BookOutputModel[]),
                    new XmlRootAttribute("Books"));
            var sw = new StringWriter();
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xmlSerializer.Serialize(sw, data, ns);
            return sw.ToString();


        }
    }
}