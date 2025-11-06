using Npgsql;

namespace kontora1
{
    public class Users
    {
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string number { get; set; }
        public string role { get; set; }

        public string Login(NpgsqlConnection connection, string inputLogin, string inputPassword)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                    Console.WriteLine("Соединение открыто.");
                }

                using (var command = new NpgsqlCommand("SELECT id_user, login, password, firstname, lastname, email, number, role FROM \"User\" WHERE login = @login AND password = @password", connection))
                {
                    command.Parameters.AddWithValue("@login", inputLogin);
                    command.Parameters.AddWithValue("@password", inputPassword);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine("Пользователь найден.");
                            id = reader.GetInt32(0);
                            login = reader.GetString(1);
                            password = reader.GetString(2);
                            firstname = reader.GetString(3);
                            lastname = reader.GetString(4);
                            email = reader.GetString(5);
                            number = reader.GetString(6);
                            role = reader.GetString(7);

                            return role.ToLower() switch
                            {
                                "admin" => "/casepage",
                                "lawyer" => "/casepage",
                                _ => null
                            };
                        }
                        else
                        {
                            Console.WriteLine("Пользователь не найден.");
                            return null; 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return null; // Ошибка подключения
            }
        }
    }
}