using Npgsql;
using System.Collections.Generic;

namespace kontora1
{
    public class Laywer
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string number { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string login { get; set; }

        // Получить всех сотрудников (роль lawyer)
        public static List<Laywer> GetAllLaywers(NpgsqlConnection connection)
        {
            var laywers = new List<Laywer>();

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var cmd = new NpgsqlCommand(
                "SELECT id_user, firstname, lastname, number, email, login, password FROM \"User\" WHERE role = 'lawyer'",
                connection);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                laywers.Add(new Laywer
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

            return laywers;
        }

        public static bool AddLaywer(NpgsqlConnection connection, Laywer laywer)
        {
            var cmd = new NpgsqlCommand(
                "INSERT INTO \"User\" (firstname, lastname, number, email, login, password, role) " +
                "VALUES (@f, @l, @n, @e, @log, @pass, 'lawyer')", connection);

            cmd.Parameters.AddWithValue("f", laywer.firstname);
            cmd.Parameters.AddWithValue("l", laywer.lastname);
            cmd.Parameters.AddWithValue("n", laywer.number);
            cmd.Parameters.AddWithValue("e", laywer.email);
            cmd.Parameters.AddWithValue("log", laywer.login);
            cmd.Parameters.AddWithValue("pass", laywer.password);

            return cmd.ExecuteNonQuery() == 1;
        }

        public static bool UpdateLaywer(NpgsqlConnection connection, Laywer laywer)
        {
            var cmd = new NpgsqlCommand(
                "UPDATE \"User\" SET firstname = @f, lastname = @l, number = @n, email = @e, " +
                "login = @log, password = @pass WHERE id_user = @id AND role = 'lawyer'", connection);

            cmd.Parameters.AddWithValue("f", laywer.firstname);
            cmd.Parameters.AddWithValue("l", laywer.lastname);
            cmd.Parameters.AddWithValue("n", laywer.number);
            cmd.Parameters.AddWithValue("e", laywer.email);
            cmd.Parameters.AddWithValue("log", laywer.login);
            cmd.Parameters.AddWithValue("pass", laywer.password);
            cmd.Parameters.AddWithValue("id", laywer.id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public static bool DeleteLaywer(NpgsqlConnection connection, int id)
        {
            var cmd = new NpgsqlCommand(
                "DELETE FROM \"User\" WHERE id_user = @id AND role = 'lawyer'", connection);
            cmd.Parameters.AddWithValue("id", id);
            return cmd.ExecuteNonQuery() == 1;
        }

        // Метод запроса документа у клиента
        public static bool RequestDocument(NpgsqlConnection connection, int lawyerId, int clientId, string description)
        {
            var cmd = new NpgsqlCommand(
                "INSERT INTO document_requests (lawyer_id, client_id, description, request_date, status) " +
                "VALUES (@lid, @cid, @desc, CURRENT_DATE, 'pending')", connection);

            cmd.Parameters.AddWithValue("lid", lawyerId);
            cmd.Parameters.AddWithValue("cid", clientId);
            cmd.Parameters.AddWithValue("desc", description);

            return cmd.ExecuteNonQuery() == 1;
        }
    }
}
