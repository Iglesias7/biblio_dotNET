using PRBD_Framework;
using System;
using prbd_1819_g04.Properties;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Reflection;

namespace prbd_1819_g04
{
    public enum AppMessages
    {
        MSG_NEW_BOOK,
        MSG_DISPLAY_BOOK,
        MSG_BOOK_CHANGED,
        MSG_BOOKCOPY_ADD,
        MSG_CATEGORY_CHANGED,
        MSG_TITLE_CHANGED,
        MSG_RENTAL_CHANGE,
        MSG_TIMER,
        MSG_CLOSE_TAB,
        MSG_BASKET_CHANGED,
        MSG_BOOKCOPY_DELETED
    }

    public partial class App : ApplicationBase
    {

        public static readonly string IMAGE_PATH = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../Pictures");
        public static Model Model { get; private set; } 
        public static User CurrentUser { get; set; }
        public static User CurrentUserActual { get; set; }

        public App()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Culture);
#if MSSQL
            Model = Model.CreateModel(DbType.MsSQL);
#else
            Model = Model.CreateModel(DbType.MySQL);
#endif
            Model.ClearDatabase();
            if (Model.Users.Count() == 0)
                Model.CreateTestData(Model);
        }
    }
}
