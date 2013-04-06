using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace OSM
{
    //public class Binding : MISP.ILibraryInterface
    //{
    //    public bool BindLibrary(MISP.Engine engine)
    //    {
    //        engine.AddFunction("open-database", "Opens a database service object connected to the local adjuster database.",
    //            (context, arguments) => { return new DatabaseService(); });
    //        return true;
    //    }
    //}

    public class DatabaseService
    {
        internal NpgsqlConnection _databaseConnection;
        internal NpgsqlTransaction _transaction;
        internal NpgsqlCommand _upsertCommand;
        internal NpgsqlCommand _deleteCommand;
        internal NpgsqlCommand _queryCommand;
        internal NpgsqlCommand _queryNameCommand;

        public DatabaseService()
        {
            _databaseConnection = new NpgsqlConnection("Server=Localhost;Port=5432;User Id=Tony;Database=Adjuster;");
            _databaseConnection.Open();

            _upsertCommand = new NpgsqlCommand("SELECT upsert(:ID, :TYPE, :NAME, :VALUE);", _databaseConnection);
            _upsertCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));
            _upsertCommand.Parameters.Add(new NpgsqlParameter("TYPE", NpgsqlTypes.NpgsqlDbType.Integer));
            _upsertCommand.Parameters.Add(new NpgsqlParameter("NAME", NpgsqlTypes.NpgsqlDbType.Text));
            _upsertCommand.Parameters.Add(new NpgsqlParameter("VALUE", NpgsqlTypes.NpgsqlDbType.Text));

            _queryCommand = new NpgsqlCommand("SELECT * FROM \"StreetData\" WHERE \"id\"=:ID;", _databaseConnection);
            _queryCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));

            _queryNameCommand = new NpgsqlCommand("SELECT * FROM \"StreetData\" WHERE \"name\" LIKE :NAME;", _databaseConnection);
            _queryNameCommand.Parameters.Add(new NpgsqlParameter("NAME", NpgsqlTypes.NpgsqlDbType.Text));

            _deleteCommand = new NpgsqlCommand("DELETE FROM \"StreetData\" WHERE \"id\"=:ID;", _databaseConnection);
            _deleteCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));

            _transaction = _databaseConnection.BeginTransaction();

            Console.WriteLine("Connected to PostgreSQL Database.\n");
        }
        
        public Entry Query(Int64 ID)
        {
            _queryCommand.Parameters["ID"].Value = ID;

            using (var Reader = _queryCommand.ExecuteReader())
            {
                if (!Reader.HasRows) return null;
                Reader.Read();
                return (new RawEntry
                {
                    id = ID,
                    type = Convert.ToInt32(Reader[1]),
                    name = Reader[3].ToString(),
                    value = Reader[2].ToString()
                }).GetEntry();
            }
        }

        public List<Entry> Query(String name)
        {
            _queryNameCommand.Parameters["NAME"].Value = name;

            using (var Reader = _queryNameCommand.ExecuteReader())
            {
                if (!Reader.HasRows) return null;
                var r = new List<Entry>();
                while (Reader.Read())
                {
                    r.Add((new RawEntry
                    {
                        id = Convert.ToInt64(Reader[0]),
                        type = Convert.ToInt32(Reader[1]),
                        name = Reader[3].ToString(),
                        value = Reader[2].ToString()
                    }).GetEntry());
                }
                return r;
            }
        }

        public void Upsert(Entry entry)
        {
            var raw = new RawEntry();
            entry.FillRawEntry(raw);
            _upsertCommand.Parameters["ID"].Value = raw.id;
            _upsertCommand.Parameters["TYPE"].Value = raw.type;
            _upsertCommand.Parameters["NAME"].Value = raw.name;
            _upsertCommand.Parameters["VALUE"].Value = raw.value;
            _upsertCommand.ExecuteNonQuery();
        }

        public void Delete(Int64 ID)
        {
            _deleteCommand.Parameters["ID"].Value = ID;
            _deleteCommand.ExecuteNonQuery();
        }

        public void CommitChanges()
        {
            if (_transaction != null) _transaction.Commit();
            _transaction = _databaseConnection.BeginTransaction();
        }

        public void DiscardChanges()
        {
            if (_transaction != null) _transaction.Rollback();
            _transaction = _databaseConnection.BeginTransaction();
        }
    }
}
