using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
   
    public partial class ProfileView : WindowBase
    {

        public ICommand Logout { get; set; }
        public ProfileView()
        {
            InitializeComponent();
            DataContext = this;

            Logout = new RelayCommand(() => {
                var LoginView = new LoginView();
                LoginView.Show();
                Application.Current.MainWindow = LoginView;
                Close();
            });

            App.Register(this, AppMessages.MSG_NEW_BOOK, () =>
            {
                var book = App.Model.Books.Create();
                App.Model.Books.Add(book);
                AddTab(book, true);
            });

            App.Register<Book>(this, AppMessages.MSG_DISPLAY_BOOK, book => {
                if (book != null)
                {
                    var tab = (from TabItem t in tabControl.Items where (string)t.Header == book.Isbn select t).FirstOrDefault();
                    if (tab == null)
                        AddTab(book, false);
                    else
                        Dispatcher.InvokeAsync(() => tab.Focus());
                }
            });

            App.Register<UserControlBase>(this, AppMessages.MSG_CLOSE_TAB, ctl => {
                var tab = (from TabItem t in tabControl.Items where t.Content == ctl select t).SingleOrDefault();
                ctl.Dispose();
                tabControl.Items.Remove(tab);
            });
        }

        private void AddTab(Book book, bool isNew)
        {
            var ctl = new BookDetailView(book, isNew);
            var tab = new TabItem()
            {
                Header = isNew ? "<new Book>" : book.Isbn,
                Content = ctl
            };

            tab.MouseDown += (o, e) => {
                if (e.ChangedButton == MouseButton.Middle &&
                    e.ButtonState == MouseButtonState.Pressed)
                {
                    tabControl.Items.Remove(o);
                    (tab.Content as UserControlBase).Dispose();
                }
            };

            tab.PreviewKeyDown += (o, e) => {
                if (e.Key == Key.W && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    tabControl.Items.Remove(o);
                    (tab.Content as UserControlBase).Dispose();
                }
            };

            tabControl.Items.Add(tab);
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

      
    }
}
