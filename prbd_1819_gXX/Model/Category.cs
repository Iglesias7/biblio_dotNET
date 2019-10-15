using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1819_g04
{
    public class Category : EntityBase<Model>
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Book> Books { get; set; } = new HashSet<Book>();

        //Constructeur
        protected Category()
        {

        }

        public bool HasBook(Book book)
        {

            var q = from bc in Model.Categories
                    where bc.Equals(book)
                    select bc;
            return q != null;
        }

        public void AddBook(Book book)
        {
            Books.Add(book);
            Model.SaveChanges();
        }

        public void RemoveBook(Book book)
        {
            Books.Remove(book);
            Model.SaveChanges();
        }

        public void Delete()
        {
            Model.Categories.Remove(this);
            Model.SaveChanges();
        }

        override
        public string ToString()
        {
            return Name;
        }

    }
}
