using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BFF.DataVirtualizingCollection.DataAccesses;
using Microsoft.Data.Sqlite;

namespace BFF.DataVirtualizingCollection.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly string PathToSimpleTestDb = $"{System.Reflection.Assembly.GetEntryAssembly().Location.Remove(System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(Path.DirectorySeparatorChar))}{Path.DirectorySeparatorChar}Databases{Path.DirectorySeparatorChar}BFF.DataVirtualizingCollection.MillionNumbers.sqlite" ;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        public IList<long> MillionNumbers { get; } = CollectionBuilder<long>
            .CreateBuilder()
            .BuildAHoardingSyncCollection(
                new RelayBasicSyncDataAccess<long>(
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
                                ret.Add((long) sqliteDataReader["Number"]);
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
                    }),
                pageSize: 100);

        public IList<int> AllPositiveIntNumbers { get; } = CollectionBuilder<int>
            .CreateBuilder()
            .BuildAHoardingSyncCollection(
                new RelayBasicSyncDataAccess<int>(
                    (offset, pageSize) =>
                    {
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading page with offset {offset}");
                        return Enumerable.Range(offset, pageSize).ToArray();
                    },
                    () =>
                    {
                        Console.WriteLine($"{nameof(AllPositiveIntNumbers)}: Loading count");
                        return int.MaxValue;
                    }),
                pageSize: 100);

        public IList<MyObject> WorkloadObjects { get; } = CollectionBuilder<MyObject>
            .CreateBuilder()
            .BuildAHoardingSyncCollection(
                new RelayBasicSyncDataAccess<MyObject>(
                    (offset, pageSize) =>
                    {
                        Console.WriteLine($"{nameof(WorkloadObjects)}: Loading page with offset {offset}");
                        return Enumerable.Range(offset, pageSize).Select(i => new MyObject(i)).ToArray();
                    },
                    () =>
                    {
                        Console.WriteLine($"{nameof(WorkloadObjects)}: Loading count");
                        return int.MaxValue;
                    }),
                pageSize: 100);

    }

    public class MyObject
    {
        byte[] _workload = new byte[12500];

        public MyObject(int number)
        {
            Number = number;
        }

        public int Number { get; }
    }
}