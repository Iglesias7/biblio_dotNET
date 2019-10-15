using MySql.Data.EntityFramework;
using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;

namespace prbd_1819_g04 {
    public enum DbType { MsSQL, MySQL }
    public enum EFDatabaseInitMode { CreateIfNotExists, DropCreateIfChanges, DropCreateAlways }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MySqlModel : Model {
        public MySqlModel(EFDatabaseInitMode initMode) : base("name=library-mysql") {
            switch (initMode) {
                case EFDatabaseInitMode.CreateIfNotExists:
                    Database.SetInitializer<MySqlModel>(new CreateDatabaseIfNotExists<MySqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateIfChanges:
                    Database.SetInitializer<MySqlModel>(new DropCreateDatabaseIfModelChanges<MySqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateAlways:
                    Database.SetInitializer<MySqlModel>(new DropCreateDatabaseAlways<MySqlModel>());
                    break;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            // see: https://blog.craigtp.co.uk/Post/2017/04/05/Entity_Framework_with_MySQL_-_Booleans,_Bits_and_%22String_was_not_recognized_as_a_valid_boolean%22_errors.
            modelBuilder.Properties<bool>().Configure(c => c.HasColumnType("bit"));
        }

        public override void Reseed(string tableName)
        {
            Database.ExecuteSqlCommand($"ALTER TABLE {tableName} AUTO_INCREMENT=1");
        }
    }

    public class MsSqlModel : Model {
        public MsSqlModel(EFDatabaseInitMode initMode) : base("name=library-mssql") {
            switch (initMode) {
                case EFDatabaseInitMode.CreateIfNotExists:
                    Database.SetInitializer<MsSqlModel>(new CreateDatabaseIfNotExists<MsSqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateIfChanges:
                    Database.SetInitializer<MsSqlModel>(new DropCreateDatabaseIfModelChanges<MsSqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateAlways:
                    Database.SetInitializer<MsSqlModel>(new DropCreateDatabaseAlways<MsSqlModel>());
                    break;
            }
        }

        public override void Reseed(string tableName)
        {
            Database.ExecuteSqlCommand($"DBCC CHECKIDENT('{tableName}', RESEED, 0)");
        }
    }

    public abstract class Model : DbContext {
        protected Model(string name) : base(name) { }

        public Model() : base("prbd_1819_g04")
        {
            Database.SetInitializer<Model>(
                new DropCreateDatabaseIfModelChanges<Model>()
            );
        }

        public static Model CreateModel(DbType type, EFDatabaseInitMode initMode = EFDatabaseInitMode.DropCreateIfChanges) {
            Console.WriteLine($"Creating model for {type}\n");
            switch (type) {
                case DbType.MsSQL:
                    return new MsSqlModel(initMode);
                case DbType.MySQL:
                    return new MySqlModel(initMode);
                default:
                    throw new ApplicationException("Undefined database type");
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalItem> RentalItems { get; set; }


        public void ClearDatabase() {
#if MSSQL
            Books.RemoveRange(Books);
            Users.RemoveRange(Users);
            BookCopies.RemoveRange(BookCopies);
            RentalItems.RemoveRange(RentalItems);
            Rentals.RemoveRange(Rentals);

            Categories.RemoveRange(Categories
                .Include(nameof(Category.Books))
                );
#else
            Categories.RemoveRange(Categories
                .Include(nameof(Category.Books))
                );
#endif
            SaveChanges();

            /**
             * Décommenter la ligne suivante pour réinitialiser le compteur de clef autoincrementée
             */
            Reseed(nameof(Users));
            Reseed(nameof(Books));
            Reseed(nameof(BookCopies));
            Reseed(nameof(Categories));
            Reseed(nameof(Rentals));
            Reseed(nameof(RentalItems));

        }

        public abstract void Reseed(string tableName);

        public void CreateTestData(Model model) {
            Console.WriteLine("Creating test data... ");
            TestDatas test = new TestDatas(model);
            test.Run();
        }

        public User CreateUser(
            string username, string password,
            string fullname, string email,
            DateTime? birthDate, Role role
        )
        {
            var user = Users.Create();
            user.UserName = username;
            user.Password = password;
            user.FullName = fullname;
            user.Email = email;
            user.BirthDate = birthDate;
            user.Role = role;
            // on ajoute le user au DbSet pour qu'il soit pris en compte par EF
            Users.Add(user);
            SaveChanges();
            return user;
        }

        public Book CreateBook(
            string isbn, string title,
            string author, string editor,
            int numCopies = 1
            )
        {
            var book = Books.Create();
            book.Isbn = isbn;
            book.Author = author;
            book.Title = title;
            book.Editor = editor;

            Books.Add(book);
            book.AddCopies(numCopies, DateTime.Now);
            
            SaveChanges();
            return book;
        }
        

        public Category CreateCategory(
            string name
        )
        {
            var category = Categories.Create();
            category.Name = name;
            Categories.Add(category);
            SaveChanges();
            return category;
        }

        public List<Book> FindBooksByText(string key)
        {
            var q1 = from bk in Books
                     where bk.Title.Contains(key)|| bk.Author.Contains(key) || bk.Editor.Contains(key) || bk.Isbn.Contains(key)
                     select bk;
            return q1.ToList();

        }

        public List<RentalItem> GetActiveRentalItems()
        {
            var q2 = from ri in RentalItems
                     where ri.ReturnDate.Equals(null)
                     select ri;

            return q2.ToList();
        }
    }
}
