using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace prbd_1819_g04 {
    public enum Role { ADMIN, MANAGER, MEMBER }

    public class User : EntityBase<Model> {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public Role Role { get; set; } 

        public RentalItem[] ActiveRentalItems {
            get
            {
                if(Basket == null)
                {
                    return null;
                }
                else
                {
                    return (from ri in Model.RentalItems
                            where ri.Rental.RentalDate == null && ri.ReturnDate == null && ri.Rental.User.UserId == UserId
                            select ri).ToArray();
                }
            }
        }
        public int Age { get; }

        public Rental Basket { get {
                var op = from ri in Rentals
                         where ri.RentalDate == null
                         select ri;
                return op.FirstOrDefault();
                }
        }

        public virtual ICollection<Rental> Rentals { get; set; } = new HashSet<Rental>();

        protected User()
        {

        }

        public Rental CreateBasket()
        {
            Rental Basket = Model.Rentals.Create();
            Basket.User = this;
            Rentals.Add(Basket);
            Model.Rentals.Add(Basket);
            Model.SaveChanges();
            return Basket;
        }

        public RentalItem AddToBasket(Book book)
        {           
            if (Basket == null)
            {
               CreateBasket();
            }
            
            RentalItem ri = null;   
            
            
            BookCopy copy = book.GetAvailableCopy();
            if (copy != null)
            {
                ri = Basket.RentCopy(copy);
            }
            
            Model.SaveChanges();
            return ri;
        }

        public void RemoveFromBasket(RentalItem rentalItem)
        {
            if(Rentals.Count > 0)
            {
                Basket.RemoveItem(rentalItem);
            }
            Model.SaveChanges();
        }

        public void ClearBasket()
        {
            if(Rentals.Count > 0)
            {
                Basket.Clear();
            }
            Model.SaveChanges();
        }

        public void ConfirmBasket()
        {
            if(Basket != null)
            {
                Basket.Confirm();
            }
            Model.SaveChanges();
        }

        public void Return(BookCopy copy)
        {
            RentalItem rentalItem = copy.RentalItems.FirstOrDefault(r => r.ReturnDate == null);
            if(rentalItem != null)
            {
                rentalItem.ReturnDate = DateTime.Now;
            }
            Model.SaveChanges();
        }
    }
}
