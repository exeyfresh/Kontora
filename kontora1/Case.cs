using Npgsql;
using System;
using System.Collections.Generic;

namespace kontora1
{
    public class Case
    {
        public int id_case { get; set; }
        public int id_user { get; set; }
        public int id_bill { get; set; }
        public int id_lawyer { get; set; }
        public DateTime data_add { get; set; }
        public string status { get; set; }
        public DateTime? data_close { get; set; }

        public static bool Create_case(NpgsqlConnection connection, Case c)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using var cmd = new NpgsqlCommand(@"INSERT INTO ""Case"" (id_user, id_lawyer, id_bill, data_add, status, data_close) 
                                        VALUES (@id_user, @id_lawyer, @id_bill, @data_add, @status, @data_close)", connection);

                cmd.Parameters.AddWithValue("id_user", c.id_user);
                cmd.Parameters.AddWithValue("id_lawyer", c.id_lawyer); // Добавьте этот параметр
                cmd.Parameters.AddWithValue("id_bill", c.id_bill);
                cmd.Parameters.AddWithValue("data_add", c.data_add);
                cmd.Parameters.AddWithValue("status", c.status);
                cmd.Parameters.AddWithValue("data_close", (object?)c.data_close ?? DBNull.Value);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении дела: {ex.Message}");
                return false;
            }
        }

        public static bool Update_status(NpgsqlConnection connection, Case updatedCase)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using (var command = new NpgsqlCommand(
                    "UPDATE \"Case\" SET id_user = @id_user, id_lawyer = @id_lawyer, id_bill = @id_bill, data_add = @data_add, status = @status, data_close = @data_close WHERE id_case = @id_case", connection))
                {
                    command.Parameters.AddWithValue("@id_case", updatedCase.id_case);
                    command.Parameters.AddWithValue("@id_user", updatedCase.id_user);
                    command.Parameters.AddWithValue("@id_lawyer", updatedCase.id_lawyer); // Добавьте этот параметр
                    command.Parameters.AddWithValue("@id_bill", updatedCase.id_bill);
                    command.Parameters.AddWithValue("@data_add", updatedCase.data_add);
                    command.Parameters.AddWithValue("@status", updatedCase.status);
                    command.Parameters.AddWithValue("@data_close", updatedCase.data_close ?? (object)DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении дела: {ex.Message}");
                return false;
            }
        }

        public static bool Delete_case(NpgsqlConnection connection, int id)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using (var command = new NpgsqlCommand("DELETE FROM \"Case\" WHERE id_case = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении дела: {ex.Message}");
                return false;
            }
        }

        public static List<Case> Get_case(NpgsqlConnection connection)
        {
            var cases = new List<Case>();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using (var command = new NpgsqlCommand("SELECT id_case, id_user, id_lawyer, id_bill, data_add, status, data_close FROM \"Case\"", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cases.Add(new Case
                            {
                                id_case = reader.GetInt32(0),
                                id_user = reader.GetInt32(1),
                                id_lawyer = reader.GetInt32(2), // Добавьте это поле
                                id_bill = reader.GetInt32(3),
                                data_add = reader.GetDateTime(4),
                                status = reader.GetString(5),
                                data_close = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении дел: {ex.Message}");
                Console.WriteLine($"Стек вызова: {ex.StackTrace}");
            }

            return cases;
        }
    }
}
