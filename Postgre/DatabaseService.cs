using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace Postgre
{
    public class Binding : MISP.ILibraryInterface
    {
        public bool BindLibrary(MISP.Engine engine)
        {
            engine.AddFunction("open-database", "Opens a database service object connected to the local adjuster database.",
                (context, arguments) => { return new DatabaseService(); });
            return true;
        }
    }

    public class DatabaseService
    {
        internal NpgsqlConnection _databaseConnection;
        internal NpgsqlTransaction _transaction;
        internal NpgsqlCommand _upsertCommand;
        internal NpgsqlCommand _deleteCommand;
        internal NpgsqlCommand _queryCommand;

        public DatabaseService()
        {
            _databaseConnection = new NpgsqlConnection("Server=Localhost;Port=5432;User Id=Tony;Database=Adjuster;");
            //_databaseConnection = new NpgsqlConnection(connectString);
            _databaseConnection.Open();

            _upsertCommand = new NpgsqlCommand("SELECT upsert(:ID, :VALUE);", _databaseConnection);
            _upsertCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));
            _upsertCommand.Parameters.Add(new NpgsqlParameter("VALUE", NpgsqlTypes.NpgsqlDbType.Text));

            _queryCommand = new NpgsqlCommand("SELECT * FROM \"OpenStreetMap\" WHERE \"id\"=:ID;", _databaseConnection);
            _queryCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));

            _deleteCommand = new NpgsqlCommand("DELETE FROM \"OpenStreetMap\" WHERE \"id\"=:ID;", _databaseConnection);
            _deleteCommand.Parameters.Add(new NpgsqlParameter("ID", NpgsqlTypes.NpgsqlDbType.Bigint));

            _transaction = _databaseConnection.BeginTransaction();

            Console.WriteLine("Connected to PostgreSQL Database.\n");
        }
        
        public String Query(Int64 ID)
        {
            _queryCommand.Parameters["ID"].Value = ID;

            using (var Reader = _queryCommand.ExecuteReader())
            {
                if (!Reader.HasRows) return null;
                Reader.Read();
                return Reader[1].ToString();
            }
        }

        public void Upsert(Int64 ID, String Value)
        {
            _upsertCommand.Parameters["ID"].Value = ID;
            _upsertCommand.Parameters["VALUE"].Value = Value;
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
