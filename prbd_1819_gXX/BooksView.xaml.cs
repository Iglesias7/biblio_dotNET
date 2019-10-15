using System;
using System.Collections.Generic;
using System.ComponentModel;
using PRBD_Framework;
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.ObjectModel;

namespace prbd_1819_g04
{

    public partial class BooksView : UserControlBase
    {

        private ObservableCollection<Book> books;
        public ObservableCollection<Book> Books
        {
            get { return books; }
            set
            {
                books = value;
                RaisePropertyChanged(nameof(Books));
            }
        }
        private ObservableCollection<Book> OldBooks;

        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                RaisePropertyChanged(nameof(Categories));
            }
        }

        private Category filterCateg;
        public Category FilterCateg
        {
            get { return filterCateg; }
            set
            {
                filterCateg = value;
                ApplyFilterCategAction();
                RaisePropertyChanged(nameof(FilterCateg));
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

        private bool btnAddEnable = false;
        public bool BtnAddEnable
        {
            get
            {
                return btnAddEnable;
            }
            set
            {
                btnAddEnable = value;
                RaisePropertyChanged(nameof(BtnAddEnable));
            }
        }

        private string filter;
        public string Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                ApplyFilterAction();
                RaisePropertyChanged(nameof(Filter));
            }
        }

        public ICommand ClearFilter { get; set; }
        public ICommand NewBookOneCommand { get; set; }
        public ICommand DetailBook { get; set; }
        public ICommand AddToBasket { get; set; }
        public ICommand CategSelected { get; set; }

        public BooksView()
        {
            InitializeComponent();
            DataContext = this;

            ManagePermissions();

            Books = new ObservableCollection<Book>(App.Model.Books.OrderBy(Book => Book.Title));
            OldBooks = Books;
            Categories = new ObservableCollection<Category>(App.Model.Categories.OrderBy(Category => Category.Name));
            Category All = App.Model.Categories.Create();
            All.CategoryId = -1;
            All.Name = "(All)";
            Categories.Insert(0, All);
            FilterCateg = All;

            CategSelected = new RelayCommand<Category>(Category => {
                FilterCateg = Category;
                });

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            ClearFilter = new RelayCommand(() => { Filter = ""; });

            NewBookOneCommand = new RelayCommand(() =>
            {
                App.NotifyColleagues(AppMessages.MSG_NEW_BOOK);
            });

            DetailBook = new RelayCommand<Book>(Book =>
            {
                App.NotifyColleagues(AppMessages.MSG_DISPLAY_BOOK, Book);
            });

            AddToBasket = new RelayCommand<Book>(Book =>
            {
                if(Book.NumAvailableCopies != 0)
                {
                    App.CurrentUser.AddToBasket(Book);
                    App.NotifyColleagues(AppMessages.MSG_BASKET_CHANGED);
                    Books = new ObservableCollection<Book>(App.Model.Books.OrderBy(book => book.Title));
                    App.Model.SaveChanges();
                }
                
            }, Book =>
            {
                if (Book == null)
                    return false;
               
                return Book.NumAvailableCopies != 0;
            });

            App.Register(this, AppMessages.MSG_BOOK_CHANGED, () =>
            {
                Books = new ObservableCollection<Book>(App.Model.Books.OrderBy(Book => Book.Title));
            });

            App.Register(this, AppMessages.MSG_CATEGORY_CHANGED, () =>
            {
                Categories = new ObservableCollection<Category>(App.Model.Categories);
                Books = new ObservableCollection<Book>(App.Model.Books.OrderBy(Book => Book.Title));
            });

            App.Register(this, AppMessages.MSG_BOOKCOPY_ADD, () =>
            {
                Books = new ObservableCollection<Book>(App.Model.Books.OrderBy(Book => Book.Title));
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

        private void ApplyFilterAction()
        {
            if(FilterCateg.CategoryId < 0)
            {
                var query = from m in App.Model.Books
                            where m.Isbn.Contains(Filter) || m.Title.Contains(Filter) || m.Author.Contains(Filter) || m.Editor.Contains(Filter)
                            select m;
                Books = new ObservableCollection<Book>(query.OrderBy(Book => Book.Title));
            }
            else
            {
                var query = from m in OldBooks
                            where m.Isbn.Contains(Filter) || m.Title.Contains(Filter) || m.Author.Contains(Filter) || m.Editor.Contains(Filter)
                            select m;
                Books = new ObservableCollection<Book>(query.OrderBy(Book => Book.Title));
            }
            
        }

        private void ApplyFilterCategAction()
        {
            if(FilterCateg.CategoryId < 0)
            {
                Filter = "";
            }
            else
            {
                var query = from m in App.Model.Books
                            where m.Categories.Any(c => c.CategoryId == FilterCateg.CategoryId)
                            select m;
                
                Books = new ObservableCollection<Book>(query.OrderBy(Book => Book.Title));
                OldBooks = Books;
            }
        }
    }
}
