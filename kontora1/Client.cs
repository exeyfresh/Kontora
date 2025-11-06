using Npgsql;

namespace kontora1
{
    public class Client
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string number { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string login { get; set; }

        // Получить всех клиентов с ролью "client"
        public static List<Client> GetAllClients(NpgsqlConnection connection)
        {
            var clients = new List<Client>();

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var cmd = new NpgsqlCommand(
                "SELECT id_user, firstname, lastname, number, email, login, password " +
                "FROM \"User\" WHERE role = 'client'", connection);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                clients.Add(new Client
                {
                    id = reader.GetInt32(0),
                    firstname = reader.GetString(1),
                    lastname = reader.GetString(2),
                    number = reader.GetString(3),
                    email = reader.GetString(4),
                    login = reader.GetString(5),
                    password = reader.GetString(6)
                });
            }

            return clients;
        }

        // Добавить клиента с ролью client
        public static bool AddClient(NpgsqlConnection connection, Client client)
        {
            var cmd = new NpgsqlCommand(
                "INSERT INTO \"User\" (firstname, lastname, number, email, login, password, role) " +
                "VALUES (@f, @l, @n, @e, @log, @pass, 'client')", connection);

            cmd.Parameters.AddWithValue("f", client.firstname);
            cmd.Parameters.AddWithValue("l", client.lastname);
            cmd.Parameters.AddWithValue("n", client.number);
            cmd.Parameters.AddWithValue("e", client.email);
            cmd.Parameters.AddWithValue("log", client.login);
            cmd.Parameters.AddWithValue("pass", client.password);

            return cmd.ExecuteNonQuery() == 1;
        }

        // Обновить клиента
        public static bool UpdateClient(NpgsqlConnection connection, Client client)
        {
            var cmd = new NpgsqlCommand(
                "UPDATE \"User\" SET firstname = @f, lastname = @l, number = @n, email = @e, " +
                "login = @log, password = @pass WHERE id_user = @id AND role = 'client'", connection);

            cmd.Parameters.AddWithValue("f", client.firstname);
            cmd.Parameters.AddWithValue("l", client.lastname);
            cmd.Parameters.AddWithValue("n", client.number);
            cmd.Parameters.AddWithValue("e", client.email);
            cmd.Parameters.AddWithValue("log", client.login);
            cmd.Parameters.AddWithValue("pass", client.password);
            cmd.Parameters.AddWithValue("id", client.id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public static bool DeleteClient(NpgsqlConnection connection, int id)
        {
            using var transaction = connection.BeginTransaction();
            try
            {
                // First delete related document requests
                var deleteRequestsCmd = new NpgsqlCommand(
                    "DELETE FROM \"DocumentRequest\" WHERE id_client = @id",
                    connection, transaction);
                deleteRequestsCmd.Parameters.AddWithValue("id", id);
                deleteRequestsCmd.ExecuteNonQuery();

                // Then delete the user
                var deleteUserCmd = new NpgsqlCommand(
                    "DELETE FROM \"User\" WHERE id_user = @id AND role = 'client'",
                    connection, transaction);
                deleteUserCmd.Parameters.AddWithValue("id", id);
                int affected = deleteUserCmd.ExecuteNonQuery();

                transaction.Commit();
                return affected == 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
