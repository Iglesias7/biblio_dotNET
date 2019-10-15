
using Microsoft.Win32;
using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
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

    public class CheckedCategory
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }

    /// <summary>
    /// Logique d'interaction pour BookDetailView.xaml
    /// </summary>
    public partial class BookDetailView : UserControlBase
    {

        private bool catModified = false;
        public bool copiesAdded = false;
        public bool loadImageAdd = false;

        public Book Book { get; set; }
        private ImageHelper imageHelper;

        public bool IsExisting { get => !isNew; }

        private bool isNew;
        public bool IsNew
        {
            get { return isNew; }
            set
            {
                isNew = value;
                RaisePropertyChanged(nameof(IsNew));
                RaisePropertyChanged(nameof(IsExisting));
            }
        }

        private bool hasPermission;
        public bool HasPermission
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

        private ObservableCollection<CheckedCategory> categories;
        public ObservableCollection<CheckedCategory> Categories
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


        public string Isbn
        {
            get { return Book.Isbn; }
            set
            {
                Book.Isbn = value;
                RaisePropertyChanged(nameof(Isbn));
                ValidateIsbn();
                App.NotifyColleagues(AppMessages.MSG_TITLE_CHANGED, string.IsNullOrEmpty(value) ? "<new Book>" : value);
            }
        }

        public string Title
        {
            get { return Book.Title; }
            set
            {
                Book.Title = value;
                RaisePropertyChanged(nameof(Title));
                ValidateTitle();
            }
        }

        public string Author
        {
            get {  return Book.Author; }
            set
            {
                Book.Author = value;
                RaisePropertyChanged(nameof(Author));
                ValidateAuthor();
            }
        }

        public string Editor
        {
            get { return Book.Editor; }
            set
            {
                Book.Editor = value;
                RaisePropertyChanged(nameof(Editor));
                ValidateEditor();
            }
        }

        public string PicturePath
        {
            get {  return Book.AbsolutePicturePath; }
            set
            {
                Book.PicturePath = value;
                RaisePropertyChanged(nameof(PicturePath));
            }
        }

        private ObservableCollection<BookCopy> bookCopies;
        public ObservableCollection<BookCopy> BookCopies
        {
            get { return bookCopies; }
            set
            {
                bookCopies = value;
                RaisePropertyChanged(nameof(BookCopies));
            }
        }

        private ObservableCollection<BookCopy> AncienBookCopies;

        public int NumAvailableCopies => Book.NumAvailableCopies;

        public ICommand CheckCB { get; set; }
        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Delete { get; set; }
        public ICommand LoadImage { get; set; }
        public ICommand ClearImage { get; set; }
        public ICommand AddBookCopies { get; set; }

#if DEBUG_USERCONTROLS_WITH_TIMER
        private Timer timer = new Timer(1000);
#endif

        public BookDetailView(Book book, bool isNew)
        {
            InitializeComponent();

            DataContext = this;

            ManagePermissions();

            Book = book;
            IsNew = isNew;
            BookCopies = new ObservableCollection<BookCopy>(book.Copies);
            AncienBookCopies = BookCopies;
            loadCateg();

            imageHelper = new ImageHelper(App.IMAGE_PATH, Book.PicturePath);
            Save = new RelayCommand(SaveAction, SaveActionEnable);
            Cancel = new RelayCommand(CancelAction, CancelActionEnable);
            Delete = new RelayCommand(DeleteAction, () => !IsNew);
            LoadImage = new RelayCommand(LoadImageAction);
            ClearImage = new RelayCommand(ClearImageAction, () => PicturePath != null);
            AddBookCopies = new RelayCommand(AddBookCopiesAction);
            CheckCB = new RelayCommand<CheckedCategory>(cat =>
            {
                catModified = true;
            });

            App.Register(this, AppMessages.MSG_CATEGORY_CHANGED, () =>
            {
                Categories = new ObservableCollection<CheckedCategory>();
                foreach (var c in App.Model.Categories)
                {
                    var cc = new CheckedCategory()
                    {
                        Name = c.Name,
                        IsChecked = Book.Categories.Contains(c)
                    };
                    categories.Add(cc);
                }
            });
        }

        private void ManagePermissions()
        {
            if (App.CurrentUser.Role == Role.ADMIN || App.CurrentUser.Role == Role.MANAGER)
            {
                HasPermission = true;
            }
            else
            {
                HasPermission = false;
            }
        }

        private void ClearImageAction()
        {
            imageHelper.Clear();
            PicturePath = imageHelper.CurrentFile;
            loadImageAdd = false;
        }

        private void LoadImageAction()
        {
            var file = new OpenFileDialog();
            if (file.ShowDialog() == true)
            {
                imageHelper.Load(file.FileName);
                PicturePath = imageHelper.CurrentFile;
            }
            loadImageAdd = true;
        }

        private void DeleteAction()
        {
            this.CancelAction();
            if (File.Exists(PicturePath))
            {
                File.Delete(PicturePath);
            }
            string message = "Do you want to delete this book?";
            string title = "Confirm Deletion";
            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(message, title, System.Windows.Forms.MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                Book.Delete();
            }
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED);
            App.NotifyColleagues(AppMessages.MSG_CLOSE_TAB, this);
        }


        private void CancelAction()
        {
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
            }

            if (IsNew)
            {
                Isbn = null;
                Title = null;
                Author = null;
                Editor = null;
                BookCopies.Clear();
                loadCateg();
                PicturePath = imageHelper.CurrentFile;
                RaisePropertyChanged(nameof(Book));
            }
            else
            {
                var change = (from b in App.Model.ChangeTracker.Entries<Book>()
                              where b.Entity == Book
                              select b).FirstOrDefault();
                if (change != null)
                {
                    change.Reload();
                    RaisePropertyChanged(nameof(Title));
                    RaisePropertyChanged(nameof(Author));
                    RaisePropertyChanged(nameof(Editor));
                    RaisePropertyChanged(nameof(PicturePath));
                }
                loadCateg();
                BookCopies = AncienBookCopies ;
            }
            catModified = false;
            copiesAdded = false;
        }

        private void loadCateg()
        {
            Categories = new ObservableCollection<CheckedCategory>();
            foreach (var c in App.Model.Categories)
            {
                var cc = new CheckedCategory()
                {
                    Name = c.Name,
                    IsChecked = Book.Categories.Contains(c)
                };
                categories.Add(cc);
            }
        }

        private void SaveAction()
        {
            if (IsNew)
            {
                App.Model.Books.Add(Book);
            }

            imageHelper.Confirm(Book.Title);
            Book.PicturePath = imageHelper.CurrentFile;

            ObservableCollection<Category> Categ = new ObservableCollection<Category>();
            foreach (CheckedCategory cc in Categories)
            {
                if (cc.IsChecked.Equals(true))
                {
                    var a = (from u in App.Model.Categories
                                where u.Name.Equals(cc.Name)
                                select u).FirstOrDefault();
                    Categ.Add(a);
                }
            }
            Category[] UpdatedCat = Categ.ToArray();
            Book.AddCategories(UpdatedCat);
            App.Model.SaveChanges();

            if (Book != null && IsNew)
            {
                foreach (BookCopy bc in BookCopies)
                {
                    Book.AddCopies(1, bc.AcquisitionDate);
                }
            }

            IsNew = false;
            catModified = false;
            App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED);
            App.NotifyColleagues(AppMessages.MSG_CLOSE_TAB, this);
        }

        private bool SaveActionEnable()
        {
            if (IsNew)
            {
                return !string.IsNullOrEmpty(Title) && !HasErrors;
            }
            else
            {
                return (copiesAdded || catModified || Book.IsModified) && !HasErrors; 
            }
        }

        private bool CancelActionEnable()
        {
            if (IsNew)
            {
                return (loadImageAdd || copiesAdded || catModified || !string.IsNullOrEmpty(Title)) && !HasErrors;
            }
            else
            {
                return (loadImageAdd || copiesAdded || catModified || Book.IsModified) && !HasErrors;
            }
        }

        private void AddBookCopiesAction()
        {
            int q = Int32.Parse(Quantity.Text);
            DateTime date = DateTime.Parse(AcquisitionDate.Text);
            
            if (!IsNew)
            {
                
                Book.AddCopies(q, date);
                BookCopies = new ObservableCollection<BookCopy>(Book.Copies);
                App.NotifyColleagues(AppMessages.MSG_BOOKCOPY_ADD);
            }
            else
            {
                BookCopy bc = App.Model.BookCopies.Create();
                bc.AcquisitionDate = date;
                for(int i = 0; i < q; ++i)
                {
                    BookCopies.Insert(i, bc);
                }
            }
            copiesAdded = true;
        }

        private object ValidateTitle()
        {
            ClearErrors();

            if (string.IsNullOrEmpty(Title))
            {
                AddError("Title", Properties.Resources.Error_Required);
            }
            else
            {
                if (Title.Length < 3)
                {
                    AddError("Title", Properties.Resources.Error_LengthGreaterEqual3);
                }
            }
            RaiseErrors();
            return !HasErrors;
        }

        private object ValidateIsbn()
        {
            ClearErrors();
            var book = (from bk in App.Model.Books
                        where Isbn.Equals(bk.Isbn)
                        select bk).FirstOrDefault();
            if (string.IsNullOrEmpty(Isbn))
            {
                AddError("Isbn", Properties.Resources.Error_Required);
            }
            else
            {
                if (book != null)
                {
                    AddError("Isbn", Properties.Resources.Error_DoesExist);
                }
            }
            RaiseErrors();
            return !HasErrors;
        }

        private object ValidateAuthor()
        {
            ClearErrors();

            if (string.IsNullOrEmpty(Author))
            {
                AddError("Author", Properties.Resources.Error_Required);
            }
            else
            {
                if (Title.Length < 3)
                {
                    AddError("Author", Properties.Resources.Error_LengthGreaterEqual3);
                }
            }
            RaiseErrors();
            return !HasErrors;
        }

        private object ValidateEditor()
        {
            ClearErrors();

            if (string.IsNullOrEmpty(Editor))
            {
                AddError("Editor", Properties.Resources.Error_Required);
            }
            else
            {
                if (Editor.Length < 3)
                {
                    AddError("Editor", Properties.Resources.Error_LengthGreaterEqual3);
                }
            }
            RaiseErrors();
            return !HasErrors;
        }

        public override void Dispose()
        {
#if DEBUG_USERCONTROLS_WITH_TIMER
            timer.Stop();
#endif
            base.Dispose();
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
                PicturePath = imageHelper.CurrentFile;
            }
        }
    }
}
