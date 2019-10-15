using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Windows.Input;

namespace prbd_1819_g04 {
    public class Book : EntityBase<Model> {


        [Key]
        public int BookId { get; set; }
        public string Isbn { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Editor { get; set; }
        public string PicturePath { get; set; }
        public int NumAvailableCopies {
            get {
                return (
                    from c in this.Model.BookCopies
                    where c.Book.BookId == BookId &&
                    (from i in c.RentalItems where i.ReturnDate == null select i).Count() == 0
                    select c
                ).Count();
            }
        }

        public virtual ICollection<BookCopy> Copies { get; set; } = new HashSet<BookCopy>();
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();

        protected Book()
        {

        }

        public void AddCategory(Category category)
        {
            Categories.Add(category);
            Model.SaveChanges();
        }

        public void AddCategories(Category[] category)
        {
            foreach(Category c in category)
            {
                AddCategory(c);
            }
            Model.SaveChanges();
        }

        public void RemoveCategory(Category category)
        {
            Categories.Remove(category);
            Model.SaveChanges();
        }

        public void AddCopies(int quantity, DateTime? date)
        {
            for(int i = 0; i < quantity; ++i)
            {
                var copy = Model.BookCopies.Create();
                copy.AcquisitionDate = date;
                copy.Book = this;
                Copies.Add(copy);
            }
            Model.SaveChanges();
        }

        public BookCopy GetAvailableCopy()
        {
            return (
                from c in this.Model.BookCopies
                where c.Book.BookId == BookId &&
                (from i in c.RentalItems where i.ReturnDate == null select i).Count() == 0
                select c
            ).FirstOrDefault();
        }

        public void DeleteCopy(BookCopy copy)
        {
            Copies.Remove(copy);
            Model.BookCopies.Remove(copy);
            //Model.Books.Remove(this);
            Model.SaveChanges();
        }

        public void Delete()
        {
            Model.BookCopies.RemoveRange(Copies);
            Model.Books.Remove(this);
            Model.SaveChanges();
        }

        override
        public string ToString()
        {
            return Title;
        }

        [NotMapped]
        public string AbsolutePicturePath
        {
            get { return PicturePath != null ? App.IMAGE_PATH + "\\" + PicturePath : null; }
        }

        //public BookCopy GetBookCopy
        //{
        //    get
        //    {
        //        return (from t in Model.BookCopies
        //                where t.Book.BookId == this.BookId 
        //                select t).FirstOrDefault();
        //    }
        //}
    }
}
