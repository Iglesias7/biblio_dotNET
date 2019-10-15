using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
using System.Globalization;

namespace prbd_1819_g04
{
    /// <summary>
    /// Logique d'interaction pour Window2.xaml
    /// </summary>
    public partial class SignupView : WindowBase
    {

        private string username;
        public string Username { get => username; set => SetProperty<string>(ref username, value, () => ValidateUsername()); }

        private string fullName;
        public string FullName { get => fullName; set => SetProperty<string>(ref fullName, value, () => ValidateFullName()); }

        private string email;
        public string Email { get => email; set => SetProperty<string>(ref email, value, () => ValidateEmail()); }

        private string birthDate;
        public string BirthDate { get => birthDate; set => SetProperty<string>(ref birthDate, value, () => ValidateBirthDate()); }

        private string password;
        public string Password { get => password; set => SetProperty<string>(ref password, value, () => ValidatePassword()); }

        private string passwordConf;
        public string PasswordConf { get => passwordConf; set => SetProperty<string>(ref passwordConf, value, () => ValidatePasswordConf()); }

        public ICommand Signup { get; set; }
        public ICommand Cancel { get; set; }

        public SignupView()
        {
            InitializeComponent();
            DataContext = this;

            Cancel = new RelayCommand(CancelAction, () => {
                return true;
            });
        }

        private void CancelAction()
        {
            var LoginView = new LoginView();
            LoginView.Show();
            Application.Current.MainWindow = LoginView;
            Close();
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
                    if (user != null)
                    {
                        AddError("Username", Properties.Resources.Error_DoesExist);
                    }
                }
            }
            RaiseErrors();
            return !HasErrors;
        }


        public bool ValidateFullName()
        {
            ClearErrors();
            if (string.IsNullOrEmpty(FullName))
            {
                AddError("Fullname", Properties.Resources.Error_Required);
            }
            RaiseErrors();
            return !HasErrors;
        }

        public bool ValidateEmail()
        {
            ClearErrors();
            var email = (from usr in App.Model.Users
                        where Username.Equals(usr.UserName)
                        select usr.Email).FirstOrDefault();


            if (string.IsNullOrEmpty(Email))
            {
                AddError("Email", Properties.Resources.Error_Required);
            }
            else
            {
                if (!IsValidEmail(email))
                {
                    AddError("Email", Properties.Resources.Error_InvalidEmail);
                }
                else
                {
                    if (email != null)
                    {
                        AddError("Email", Properties.Resources.Error_DoesExist);
                    }
                }
            }
            RaiseErrors();
            return !HasErrors;
        }


        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public bool ValidateBirthDate()
        {
            ClearErrors();
            //var user = App.Model.Users.Find(Username);
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
                    //if (user == null)
                    //{
                    //    AddError("Username", Properties.Resources.Error_DoesNotExist);
                    //}
                }
            }
            RaiseErrors();
            return !HasErrors;
        }

        public bool ValidatePassword()
        {
            ClearErrors();
            if (string.IsNullOrEmpty(Password))
            {
                AddError("Password", Properties.Resources.Error_Required);
            }
            else
            {
                if (Password.Length < 8)
                {
                    AddError("Password", Properties.Resources.Error_LengthGreaterEqual3);
                }
                else
                {
                    //if (user == null)
                    //{
                    //    AddError("Username", Properties.Resources.Error_DoesNotExist);
                    //}
                }
            }
            RaiseErrors();
            return !HasErrors;
        }

        public bool ValidatePasswordConf()
        {
            ClearErrors();
            if (Password != PasswordConf)
            {
                AddError("PasswordConf", Properties.Resources.Error_LengthGreaterEqual3);
            }
            RaiseErrors();
            return !HasErrors;
        }

    }
}
