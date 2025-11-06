using Npgsql;

namespace kontora1.Pages
{
    public class Bill
    {
        public int id_bill { get; set; }
        public int id_user { get; set; }
        public double price { get; set; }
        public string is_paid { get; set; }
        public DateTime date_issued { get; set; } = DateTime.Now; // значение по умолчанию
        public DateTime? date_paid { get; set; } // Nullable

        public static List<Bill> GetBills(NpgsqlConnection connection)
        {
            var list = new List<Bill>();
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using var cmd = new NpgsqlCommand(@"SELECT id_bill, id_user, price, is_paid, date_issued, date_paid FROM ""Bill""", connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Bill
                    {
                        id_bill = reader.GetInt32(0),
                        id_user = reader.GetInt32(1),
                        price = reader.GetDouble(2),
                        is_paid = reader.GetString(3),
                        date_issued = reader.GetDateTime(4),
                        date_paid = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении счетов: {ex.Message}");
            }

            return list;
        }

        public static bool CreateBill(NpgsqlConnection connection, Bill b)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO ""Bill"" (id_user, price, is_paid, date_issued) 
                  VALUES (@id_user, @price, @is_paid, @date_issued)", connection);

                cmd.Parameters.AddWithValue("id_user", b.id_user);
                cmd.Parameters.AddWithValue("price", b.price);
                cmd.Parameters.AddWithValue("is_paid", b.is_paid ?? "не оплачен");
                cmd.Parameters.AddWithValue("date_issued", b.date_issued);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании счета: {ex.Message}");
                return false;
            }
        }

        public static bool PayBill(NpgsqlConnection connection, int id)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using var cmd = new NpgsqlCommand(
                    @"UPDATE ""Bill"" SET is_paid = 'Оплачен', date_paid = @date_paid WHERE id_bill = @id", connection);
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("date_paid", DateTime.Now);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при оплате счета: {ex.Message}");
                return false;
            }
        }
        public static bool DeleteBill(NpgsqlConnection connection, int id)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using var cmd = new NpgsqlCommand(
                    @"DELETE FROM ""Bill"" WHERE id_bill = @id", connection);
                cmd.Parameters.AddWithValue("id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении счета: {ex.Message}");
                return false;
            }
        }
    }

}
