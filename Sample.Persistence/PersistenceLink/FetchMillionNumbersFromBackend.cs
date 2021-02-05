using System.Collections.Generic;
using System.IO;
using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.PersistenceLink;
using Microsoft.Data.Sqlite;

namespace BFF.DataVirtualizingCollection.Sample.Persistence.PersistenceLink
{
    internal class FetchMillionNumbersFromBackend : IFetchMillionNumbersFromBackend
    {
        private static readonly string PathToSimpleTestDb = $"{System.Reflection.Assembly.GetEntryAssembly()?.Location.Remove(System.Reflection.Assembly.GetEntryAssembly()?.Location.LastIndexOf(Path.DirectorySeparatorChar) ?? 0)}{Path.DirectorySeparatorChar}Databases{Path.DirectorySeparatorChar}BFF.DataVirtualizingCollection.MillionNumbers.sqlite";
        public long[] FetchPage(int pageOffset, int pageSize)
        {
            using var sqliteConnection =
                new SqliteConnection(
                    new SqliteConnectionStringBuilder
                        {
                            DataSource = PathToSimpleTestDb
                        }
                        .ToString());
            sqliteConnection.Open();
            sqliteConnection.BeginTransaction();
            var sqliteCommand = sqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"SELECT Number FROM Numbers LIMIT {pageSize} OFFSET {pageOffset};";
            var sqliteDataReader = sqliteCommand.ExecuteReader();
            IList<long> ret = new List<long>();
            while (sqliteDataReader.Read())
                ret.Add((long)sqliteDataReader["Number"]);
            return ret.ToArray();
        }

        public int CountFetch()
        {
            using var sqliteConnection =
                new SqliteConnection(
                    new SqliteConnectionStringBuilder
                        {
                            DataSource = PathToSimpleTestDb
                        }
                        .ToString());
            sqliteConnection.Open();
            var sqliteCommand = sqliteConnection.CreateCommand();
            sqliteCommand.CommandText = "SELECT Count(*) FROM Numbers;";
            return (int)(long)sqliteCommand.ExecuteScalar();
        }
    }
}