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
    public partial class BasketView : UserControlBase
    {

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get
            {
                return users;
            }
            set
            {
                users = value;
                RaisePropertyChanged(nameof(Users));
            }
        }

        private User selectedUser;
        public User SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                App.CurrentUser = SelectedUser;
                getBooksByUser();
                RaisePropertyChanged(nameof(SelectedUser));
            }
        }

        private ObservableCollection<RentalItem> bookUsers;
        public ObservableCollection<RentalItem> BookUsers
        {
            get
            {
                return bookUsers;
            }
            set
            {
                bookUsers = value;
                RaisePropertyChanged(nameof(BookUsers));
            }
        }

        private string hasPermission;
        public string HasPermission
        {
            get
            {
                return hasPermission;
            }
            set
            {
                hasPermission = value;
                RaisePropertyChanged(nameof(HasPermission));
            }
        }

        public ICommand Delete { get; set; }
        public ICommand Clear { get; set; }
        public ICommand Confirm { get; set; }

        public BasketView()
        {
            InitializeComponent();
            DataContext = this;

            ManagePermissions();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            Users = new ObservableCollection<User>(App.Model.Users);
            BookUsers = new ObservableCollection<RentalItem>();
            SelectedUser = App.CurrentUser;

            Delete = new RelayCommand<RentalItem>(RentalItem =>
            {
                BookUsers.Remove(RentalItem);
                App.CurrentUser.RemoveFromBasket(RentalItem);
                App.Model.SaveChanges();
                App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED);
            });

            Clear = new RelayCommand(() => {
                App.CurrentUser.ClearBasket();
                 BookUsers.Clear();
                App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED);
            });

            Confirm = new RelayCommand(() => {
                App.CurrentUser.ConfirmBasket();
                BookUsers.Clear();
                App.NotifyColleagues(AppMessages.MSG_RENTAL_CHANGE);
            });

            App.Register(this, AppMessages.MSG_BASKET_CHANGED, () =>
            {
                getBooksByUser() ;
            });
        }

        private void ManagePermissions()
        {
            if (App.CurrentUserActual.Role == Role.ADMIN || App.CurrentUserActual.Role == Role.MANAGER)
            {
                HasPermission = "Visible";
            }
            else
            {
                HasPermission = "Collapsed";
            }
        }

        private void getBooksByUser()
        {
            if (BookUsers != null)
            {
                BookUsers.Clear();
            }

            RentalItem[] ri = SelectedUser.ActiveRentalItems;
            if (ri != null)
            {
                foreach (RentalItem r in ri)
                {
                    BookUsers.Add(r);
                }
            }
        }
    }
}
