using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1819_g04 { 
    public class RentalItem
    {
        [Key]
        public int RentalItemId { get; set; }
        public DateTime? ReturnDate { get; set; }

        [Required]
        public virtual Rental Rental { get; set; }

        [Required]
        public virtual BookCopy BookCopy { get; set; }

        protected RentalItem()
        {

        }

        public void DoReturn()
        {
            ReturnDate = DateTime.Now;
            Rental.RemoveItem(this);
        }

        public void CancelReturn()
        {
            ReturnDate = null;
        }

        override
        public string ToString()
        {
            return BookCopy.Book.ToString();
        }


    }
}
