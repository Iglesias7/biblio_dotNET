using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace prbd_1819_g04
{
    
    public partial class RentalsView : UserControlBase
    {
        private ObservableCollection<Rental> rentals;
        public ObservableCollection<Rental> Rentals
        {
            get
            {
                return rentals;
            }
            set
            {
                rentals = value;
                RaisePropertyChanged(nameof(Rentals));
            }
        }

        private Rental rentalSelected;
        public Rental RentalSelected
        {
            get
            {
                return rentalSelected;
            }
            set
            {
                rentalSelected = value;
                RaisePropertyChanged(nameof(RentalSelected));
            }
        }

        private ObservableCollection<RentalItem> rentalItems;
        public ObservableCollection<RentalItem> RentalItems
        {
            get
            {
                return rentalItems;
            }
            set
            {
                rentalItems = value;
                RaisePropertyChanged(nameof(RentalItems));
            }
        }

        private bool enable = true;
        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                enable = value;
                RaisePropertyChanged(nameof(Enable));
            }
        }

        public ICommand Delete { get; set; }
        public ICommand Return { get; set; }
        public ICommand LoadItems { get; set; }

        public RentalsView()
        {
            InitializeComponent();
            DataContext = this;
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            lsRental();

            LoadItems = new RelayCommand<Rental>(Rental =>
            {
                RentalSelected = Rental;
                if (RentalItems != null)
                {
                    RentalItems.Clear();
                }
                RentalItems = new ObservableCollection<RentalItem>(Rental.Items);
            });


            Delete = new RelayCommand<RentalItem>(RentalItem =>
            {
                RentalSelected.RemoveItem(RentalItem);
                lsRental();
                RentalItems = new ObservableCollection<RentalItem>(RentalSelected.Items);
                App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED);
            });

            Return = new RelayCommand<RentalItem>(RentalItem =>
            {
                App.CurrentUser.Return(RentalItem.BookCopy);
                RentalItems.RefreshFromModel(RentalSelected.Items);
                App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED);
            });

            if(App.CurrentUserActual.Role == Role.MEMBER)
            {
                Enable = false;
            }

            App.Register(this, AppMessages.MSG_RENTAL_CHANGE, () =>
            {
                lsRental();
            });
        }

        private void lsRental()
        {
            if (App.CurrentUser.Role == Role.ADMIN || App.CurrentUser.Role == Role.MANAGER)
            {
                var q = (from b in App.Model.Rentals
                         where b.RentalDate != null && b.Items.Count != 0
                         select b).ToList();
                Rentals = new ObservableCollection<Rental>(q);
            }
            else
            {
                var q = (from b in App.Model.Rentals
                         where b.RentalDate != null && b.User.UserName == App.CurrentUser.UserName && b.Items.Count != 0
                         select b).ToList();
                Rentals = new ObservableCollection<Rental>(q);
            }
        }
    }
}
