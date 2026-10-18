using Npgsql;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.Infraestructura.Persistencia.Helpers
{
[ExcludeFromCodeCoverage]
    public static class RepositoryDbHelper
    {
        public static int ExecuteNonQuery(string connectionString, string query, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(connectionString);
            using var cmd = new NpgsqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static int ExecuteNonQuery(string connectionString, NpgsqlCommand command)
        {
            using var connection = new NpgsqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string connectionString, NpgsqlCommand command)
        {
            using var connection = new NpgsqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();
            return command.ExecuteScalar();
        }

        public static NpgsqlDataReader ExecuteReader(string connectionString, string query, params NpgsqlParameter[] parameters)
        {
            var connection = new NpgsqlConnection(connectionString);
            var command = new NpgsqlCommand(query, connection);

            if (parameters != null)
                command.Parameters.AddRange(parameters);

            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static T? ExecuteReaderSingle<T>(string connectionString, NpgsqlCommand command, Func<NpgsqlDataReader, T> mapper)
        {
            using var connection = new NpgsqlConnection(connectionString);
            command.Connection = connection;
            connection.Open();

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return mapper(reader);
            }

            return default;
        }
    }
}
