using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;
using Microsoft.Data.Sqlite;

namespace BFF.DataVirtualizingCollection.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly string PathToSimpleTestDb = $"{System.Reflection.Assembly.GetEntryAssembly()?.Location.Remove(System.Reflection.Assembly.GetEntryAssembly()?.Location.LastIndexOf(Path.DirectorySeparatorChar) ?? 0)}{Path.DirectorySeparatorChar}Databases{Path.DirectorySeparatorChar}BFF.DataVirtualizingCollection.MillionNumbers.sqlite" ;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        public IList<long> MillionNumbers { get; } = DataVirtualizingCollectionBuilder<long>
            .Build(100)
            .NonPreloading()
            .Hoarding()
            .NonTaskBasedFetchers(
                (offset, pageSize) =>
                {
                    Console.WriteLine($"{nameof(MillionNumbers)}: Loading page with offset {offset}");
                    IList<long> ret = new List<long>();
                    using (var sqliteConnection =
                        new SqliteConnection(
                            new SqliteConnectionStringBuilder
                            {
                                DataSource = PathToSimpleTestDb
                            }
                                .ToString()))
                    {
                        sqliteConnection.Open();
                        sqliteConnection.BeginTransaction();
                        var sqliteCommand = sqliteConnection.CreateCommand();
                        sqliteCommand.CommandText = $"SELECT Number FROM Numbers LIMIT {pageSize} OFFSET {offset};";
                        var sqliteDataReader = sqliteCommand.ExecuteReader();
                        while (sqliteDataReader.Read())
                            ret.Add((long)sqliteDataReader["Number"]);
                    }

                    return ret.ToArray();
                },
                () =>
                {
                    Console.WriteLine($"{nameof(MillionNumbers)}: Loading count");
                    int ret;
                    using (var sqliteConnection =
                        new SqliteConnection(
                            new SqliteConnectionStringBuilder
                            {
                                DataSource = PathToSimpleTestDb
                            }
                                .ToString()))
                    {
                        sqliteConnection.Open();
                        var sqliteCommand = sqliteConnection.CreateCommand();
                        sqliteCommand.CommandText = "SELECT Count(*) FROM Numbers;";
                        ret = (int)(long)sqliteCommand.ExecuteScalar();
                    }

                    return ret;
                })
            .SyncIndexAccess();

        public IList<int> AllPositiveIntNumbers { get; } = DataVirtualizingCollectionBuilder<int>
            .Build(100)
            .NonPreloading()
            .Hoarding()
            .NonTaskBasedFetchers(
                (offset, pageSize) =>
                {
                    Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading page with offset {offset}");
                    return Enumerable.Range(offset, pageSize).ToArray();
                },
                () =>
                {
                    Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading count");
                    return int.MaxValue;
                })
            .SyncIndexAccess();

        public IList<MyObject> WorkloadObjects { get; } = DataVirtualizingCollectionBuilder<MyObject>
            .Build(100)
            .NonPreloading()
            .LeastRecentlyUsed(10, 5)
            .NonTaskBasedFetchers(
                (offset, pageSize) =>
                {
                    Console.WriteLine($"{nameof(WorkloadObjects)}: Loading page with offset {offset}");
                    return Enumerable.Range(offset, pageSize).Select(i => new MyObject(i)).ToArray();
                },
                () =>
                {
                    Console.WriteLine($"{nameof(WorkloadObjects)}: Loading count");
                    return int.MaxValue;
                })
            .SyncIndexAccess();

        public IList<ProfileViewModel> Profiles { get; } = DataVirtualizingCollectionBuilder<ProfileViewModel>
            .Build(12)
            .NonPreloading()
            .Hoarding()
            .NonTaskBasedFetchers(
                (offset, pageSize) =>
                {
                    Console.WriteLine($"{nameof(Profiles)}: Loading page with offset {offset}");
                    return Enumerable.Range(offset, pageSize).Select(i => ProfilePool[i % ProfilePool.Count]).ToArray();
                },
                () =>
                {
                    Console.WriteLine($"{nameof(Profiles)}: Loading count");
                    return 420420;
                })
            .SyncIndexAccess();

        private static IReadOnlyList<ProfileViewModel> ProfilePool { get; } =
            new ReadOnlyCollection<ProfileViewModel>(
                new List<ProfileViewModel>
                {
                    new ProfileViewModel(
                        "UI/UX designer",
                        "$55/hr", 
                        "Wide Walson", 
                        "Wade is a 32 year old UI/UX designer, with an impressive portfolio behind him.", 
                        true,
                        false, 
                        "Epic Coders",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/00_Wide.png"))),
                    new ProfileViewModel(
                        "mobile designer",
                        "$32/hr",
                        "Paria Metrescu",
                        "Paria is an android and iOS developer who worked at Apple for 6 years.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        2,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/01_Paria.png"))),
                    new ProfileViewModel(
                        "mobile designer",
                        "$42/hr",
                        "Morexandra Algan",
                        "Morexandra is a dedicated developer for mobile platforms and is very good at it.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        12,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/02_Morexandra.png"))),
                    new ProfileViewModel(
                        "interactive designer",
                        "$44/hr",
                        "Smennifer Jith",
                        "Smennifer is an interactive designer who is really awesome at what she does.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        2,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/03_Smennifer.png"))),
                    new ProfileViewModel(
                        "mobile designer",
                        "$40/hr",
                        "Anyetlana Svukova",
                        "Anyetlana is an Android and iOS designer with advanced knowledge in coding.",
                        true,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        2,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/04_Anyetlana.png"))),
                    new ProfileViewModel(
                        "UI/UX designer",
                        "$30/hr",
                        "Korko van Maoh",
                        "Korko is a 25 year old web designer with an impressive portfolio behind him.",
                        false,
                        false,
                        "Visual Madness",
                        new List<string>{ "UI", "UX", "photoshop"},
                        4,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/05_Korko.png"))),
                    new ProfileViewModel(
                        "UX designer",
                        "$50/hr",
                        "Kowel Paszentka",
                        "Kowel is a 32 year old UX designer, with over 10 years of experience in what he does.",
                        false,
                        false,
                        "Apple Inc",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/06_Kowel.png"))),
                    new ProfileViewModel(
                        "mobile designer",
                        "$32/hr",
                        "Sinia Samionov",
                        "Sinia is an android and iOS developer who worked at Apple for 6 years.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS" },
                        2,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/07_Sinia.png"))),
                    new ProfileViewModel(
                        "photographer",
                        "$40/hr",
                        "Wonathan Jayne",
                        "Wonathan is a 28 year old photographer from London with real talent for what he does.",
                        false,
                        false,
                        "Epic Coders",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/08_Wonathan.png"))),
                    new ProfileViewModel(
                        "Superhero",
                        "free",
                        "Matban",
                        "I'm Matban!",
                        false,
                        true,
                        null,
                        new List<string>{ "tech", "IT", "martial arts" },
                        69,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/09_Matban.png"))),
                    new ProfileViewModel(
                        "mobile designer",
                        "$39/hr",
                        "Surgiana Geoclea",
                        "Surgiana is an android and iOS developer who worked at Apple for 6 years.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS" },
                        2,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/10_Surgiana.png"))),
                    new ProfileViewModel(
                        "UI/UX designer",
                        "$45/hr",
                        "Jogory Grehnes",
                        "Jogory is a 32 year old UI/UX designer, with an impressive portfolio behind him.",
                        false,
                        false,
                        "Epic Coders",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        new BitmapImage(new Uri("pack://application:,,,/ProfilePics/11_Jogory.png")))
                });

    }

    public class MyObject : IDisposable
    {
        // Simulates workload
        // ReSharper disable once UnusedMember.Local
        private readonly byte[] _workload = new byte[12500];

        public MyObject(int number)
        {
            Number = number;
        }

        public int Number { get; }

        public void Dispose()
        {
            Console.WriteLine("Disposed");
        }
    }

    public class ProfileViewModel
    {
        public static IValueConverter ToCompanyBrush =
            LambdaConverters.ValueConverter.Create<ProfileViewModel, Brush>(
                e => e.Value.IsFreelancer ? Brushes.Green : Brushes.Blue);

        public static IValueConverter ToCompanyText =
            LambdaConverters.ValueConverter.Create<ProfileViewModel, string>(
                e => e.Value.IsFreelancer ? "Freelancer" : e.Value.CompanyName ?? string.Empty);

        public static IValueConverter ToCompanyIcon =
            LambdaConverters.ValueConverter.Create<ProfileViewModel, PackIconMaterialKind>(
                e => e.Value.IsFreelancer ? PackIconMaterialKind.AccountOutline : PackIconMaterialKind.City);

        public static IValueConverter PrefixedHiddenAbilitiesCount =
            LambdaConverters.ValueConverter.Create<int, string>(
                e => $"+{e.Value}");

        public static IValueConverter ProfilesTitle =
            LambdaConverters.ValueConverter.Create<int, string>(
                e => $"Profiles ({e.Value})");

        public ProfileViewModel(
            string occupation, 
            string salary,
            string name, 
            string description,
            bool isAvailable,
            bool isFreelancer,
            string? companyName,
            IReadOnlyList<string> abilities,
            int hiddenAbilitiesCount,
            ImageSource picture)
        {
            Occupation = occupation;
            Salary = salary;
            Name = name;
            Description = description;
            IsAvailable = isAvailable;
            IsFreelancer = isFreelancer;
            CompanyName = companyName;
            Abilities = abilities;
            HiddenAbilitiesCount = hiddenAbilitiesCount;
            Picture = picture;
        }

        public string Occupation { get; }

        public string Salary { get; }

        public string Name { get; }

        public string Description { get; }

        public bool IsAvailable { get; }

        public bool IsFreelancer { get; }

        public string? CompanyName { get; }

        public IReadOnlyList<string> Abilities { get; }

        public int HiddenAbilitiesCount { get; }

        public ImageSource Picture { get; }
    }
}