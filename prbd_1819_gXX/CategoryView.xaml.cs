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
    public partial class CategoryView : UserControlBase
    {

        private ObservableCollection<Book> books;
        public ObservableCollection<Book> Books
        {
            get
            {
                return books;
            }
            set
            {
                books = value;
                RaisePropertyChanged(nameof(Books));
            }
        }

        private ObservableCollection<Category> categories;
        public ObservableCollection<Category> Categories
        {
            get
            {
                return categories;
            }
            set
            {
                categories = value;
                RaisePropertyChanged(nameof(Categories));
            }
        }

        private Category oneCategory;
        public Category OneCategory
        {
            get
            {
                return oneCategory;
            }
            set
            {
                oneCategory = value;
                RaisePropertyChanged(nameof(OneCategory));
                TextBlockCateg = oneCategory.Name;
            }
        }

        private string textBlockCateg;
        public string TextBlockCateg
        {
            get
            {
                return textBlockCateg;
            }
            set
            {
                textBlockCateg = value;
                RaisePropertyChanged(nameof(TextBlockCateg));
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

        public ICommand Add { get; set; }
        public ICommand Update { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Delete { get; set; }

        public CategoryView()
        {
            InitializeComponent();
            DataContext = this;

            ManagePermissions();
          
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            Books = new ObservableCollection<Book>(App.Model.Books);
            Categories = new ObservableCollection<Category>(App.Model.Categories.OrderBy(Category => Category.Name));

            Add = new RelayCommand(AddAction, () => {
                
                return OneCategory == null && TextBlockCateg != null && !ExistsAlready();
            });

            Update = new RelayCommand(UpdateAction, () => {
                return OneCategory != null && TextBlockCateg != null;
            });

            Delete = new RelayCommand(DeleteAction, () => {
                return OneCategory != null && TextBlockCateg == OneCategory.Name;
            });

            Cancel = new RelayCommand(CancelAction, () =>
            {
                return TextBlockCateg != null;
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

        private bool ExistsAlready()
        {
            bool q = false;

            foreach (var c in Categories)
            {
                if (TextBlockCateg.CompareTo(c.Name) == 0 )
                    q = true;
            }
            return q;
        }

        private void AddAction()
        {
            Category c = App.Model.Categories.Create();
            c.Name = TextBlockCateg;
            App.Model.Categories.Add(c);
            TextBlockCateg = "";
            App.Model.SaveChanges();
            
            Categories = new ObservableCollection<Category>(App.Model.Categories.OrderBy(Category => Category.Name));
            App.NotifyColleagues(AppMessages.MSG_CATEGORY_CHANGED);
        }

        private void UpdateAction()
        {
            OneCategory.Name = TextBlockCateg;
            TextBlockCateg = "";
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_CATEGORY_CHANGED);
            Categories = new ObservableCollection<Category>(App.Model.Categories.OrderBy(Category => Category.Name));
        }

        private void DeleteAction()
        {
            App.Model.Categories.Remove(OneCategory);
            TextBlockCateg = "";
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_CATEGORY_CHANGED);
            Categories = new ObservableCollection<Category>(App.Model.Categories.OrderBy(Category => Category.Name));
        }

        private void CancelAction()
        {
            TextBlockCateg = "";
        }
    }
}
