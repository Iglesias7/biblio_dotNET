using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1819_g04
{
    public class Rental : EntityBase<Model>
    {
        [Key]
        public int RentalId { get; set; }
        public DateTime? RentalDate { get; set; }
        public int NumOpenItems
        {//le nombre de livres que l'utilisateur n'a pas encore retourné
            get
            {
                return(
                        from ri in Items
                         where ri.ReturnDate.Equals(null)
                         select ri
                        ).Count();
            }
        }

        public virtual User User { get; set; }

        public virtual ICollection<RentalItem> Items { get; set; } = new HashSet<RentalItem>();


        protected Rental()
        {

        }

        public RentalItem RentCopy(BookCopy copy)
        {
            RentalItem rental = Model.RentalItems.Create();
            Model.RentalItems.Add(rental);
            rental.BookCopy = copy;
            rental.ReturnDate = null;
            rental.Rental = this;

            copy.RentalItems.Add(rental);
            
            this.Items.Add(rental);

            return rental;
        }

        public void RemoveCopy(BookCopy copy)
        {
            Model.BookCopies.Remove(copy);
            Model.SaveChanges();
        }

        public void RemoveItem(RentalItem item)
        {
            Model.RentalItems.Remove(item);
            Items.Remove(item);
            Model.SaveChanges();
        }

        public void Return(RentalItem item)
        {
            item.DoReturn();
            Model.SaveChanges();
        }

        public void Confirm()
        {
            //Attention : après l'appel à cette méthode sur un panier courant, celui-ci n'existe plus (puisqu'il a été sauvegardé)
            if(RentalDate == null)
            {
                RentalDate = DateTime.Now;
            }
            Model.SaveChanges();

        }

        public void Clear()
        {
            Model.RentalItems.RemoveRange(Items);
            Items.Clear();
        }

        override
        public string ToString()
        {
            return RentalId.ToString();
        }
    }
}
