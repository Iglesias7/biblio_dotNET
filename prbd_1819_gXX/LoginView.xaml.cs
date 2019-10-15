using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using prbd_1819_g04.Properties;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace prbd_1819_g04
{
    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class LoginView : WindowBase
    {
        private string username;
        public string Username { get => username; set => SetProperty<string>(ref username, value, () => ValidateUsername()); }

        private string password;
        public string Password { get => password; set => SetProperty<string>(ref password, value, () => ValidatePassword()); }

        public ICommand Login { get; set; }
        public ICommand Signup { get; set; }
        public ICommand Cancel { get; set; }

        public LoginView()
        {
            InitializeComponent();
            DataContext = this;
            Login = new RelayCommand(LoginAction, () => {
                        return username != null && password != null && !HasErrors;
            });

            Signup = new RelayCommand(SignupAction, () => {
                return true ;
            });

            Cancel = new RelayCommand(() => Close());
        }

        private void LoginAction()
        {
            if (ValidateUsername() && ValidatePassword())
            { // si aucune erreurs
                var user = (from usr in App.Model.Users
                            where Username.Equals(usr.UserName)
                            select usr).FirstOrDefault();

                App.CurrentUser = user; // le membre connecté devient le membre courant
                App.CurrentUserActual = user;
                ShowProfileView(); // ouverture de la fenêtre principale
                Close(); // fermeture de la fenêtre de login
            }
        }

        private void SignupAction()
        {
            var SignupView = new SignupView();
            SignupView.Show();
            Application.Current.MainWindow = SignupView;
            Close();
        }

        private static void ShowProfileView()
        {
            var ProfileView = new ProfileView();
            ProfileView.Show();
            
            Application.Current.MainWindow = ProfileView;
            
        }

        public bool ValidateUsername()
        {
            ClearErrors();
            var user = (from usr in App.Model.Users
                         where Username.Equals(usr.UserName)
                         select usr).FirstOrDefault();

            
            if (string.IsNullOrEmpty(Username))
            {
                AddError("Username", Properties.Resources.Error_Required);
            }
            else
            {
                if (Username.Length < 3)
                {
                    AddError("Username", Properties.Resources.Error_LengthGreaterEqual3);
                }
                else
                {
                    if (user == null)
                    {
                        AddError("Username", Properties.Resources.Error_DoesNotExist);
                    }
                }
            }
            RaiseErrors();
            return !HasErrors;
        }


        public bool ValidatePassword()
        {
            ClearErrors();
            var userPsw = (from usr in App.Model.Users
                        where Username.Equals(usr.UserName)
                        select usr.Password).FirstOrDefault();

            if (string.IsNullOrEmpty(Password))
            {
                AddError("Password", Properties.Resources.Error_Required);
            }
            else if(userPsw != Password)
            {
                AddError("Password", Properties.Resources.Error_MustMatchPassword);
            }
           
            RaiseErrors();
            return !HasErrors;
        }
    }
}
