using Npgsql;
using System.Collections.Generic;

namespace kontora1.Pages
{
    public class Reports
    {
        public class IncomeReport
        {
            public List<BillItem> Bills { get; set; } = new List<BillItem>();
            public double TotalIncome { get; set; }
            public int TotalBills { get; set; }
            public int TotalPaidBills { get; set; }
            public string Period { get; set; }

            public class BillItem
            {
                public int BillNumber { get; set; }
                public double Amount { get; set; }
            }
        }
        public class DebtReportItem
        {
            public int BillNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public double Amount { get; set; }
        }
        public class DebtReport
        {
            public string Period { get; set; }
            public List<DebtReportItem> Items { get; set; } = new List<DebtReportItem>();
            public double TotalDebt { get; set; }
            public int TotalDebts => Items.Count; // Добавленное свойство
        }


        public class LawyerWorkReportItem
        {
            public int LawyerId { get; set; }
            public string LawyerName { get; set; }
            public int TotalCases { get; set; }
            public int CreatedCases { get; set; }
            public int InProgressCases { get; set; }
            public int ClosedCases { get; set; }
        }

        public class LawyerWorkReport
        {
            public List<LawyerWorkReportItem> Items { get; set; } = new List<LawyerWorkReportItem>();
            public int TotalAllCases { get; set; }
            public int TotalCreated { get; set; }
            public int TotalInProgress { get; set; }
            public int TotalClosed { get; set; }
            public string Period { get; set; }
            public int? SelectedLawyerId { get; set; }
        }

        public static IncomeReport GenerateIncomeReport(NpgsqlConnection connection, DateTime from, DateTime to)
        {
            var report = new IncomeReport
            {
                Period = $"{from:dd.MM.yyyy} - {to:dd.MM.yyyy}"
            };

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                // Получаем список оплаченных счетов и сразу считаем общую сумму
                var billsCmd = new NpgsqlCommand(@"
            SELECT id_bill, price
            FROM ""Bill""
            WHERE is_paid = 'Оплачен'
              AND date_paid >= @from AND date_paid <= @to
            ORDER BY id_bill", connection);

                billsCmd.Parameters.AddWithValue("@from", from);
                billsCmd.Parameters.AddWithValue("@to", to);

                using (var reader = billsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        report.Bills.Add(new IncomeReport.BillItem
                        {
                            BillNumber = reader.GetInt32(0),
                            Amount = reader.GetDouble(1)
                        });
                        report.TotalIncome += reader.GetDouble(1);
                    }
                    // Количество оплаченных счетов = количество строк в результате
                    report.TotalPaidBills = report.Bills.Count;
                }

                // Получаем общее количество счетов за период (по дате выставления)
                var statsCmd = new NpgsqlCommand(@"
    SELECT COUNT(*) AS total_bills
    FROM ""Bill""
    WHERE date_issued >= @from AND date_issued <= @to", connection);

                statsCmd.Parameters.AddWithValue("@from", from);
                statsCmd.Parameters.AddWithValue("@to", to);

                using (var reader = statsCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        report.TotalBills = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации отчета о доходах: {ex.Message}");
            }

            return report;
        }

        public static DebtReport GenerateDebtReport(NpgsqlConnection connection, DateTime from, DateTime to)
        {
            var report = new DebtReport
            {
                Period = $"{from:dd.MM.yyyy} - {to:dd.MM.yyyy}"
            };

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                var cmd = new NpgsqlCommand(@"
            SELECT b.id_bill, u.firstname, u.lastname, b.price
            FROM ""Bill"" b
            JOIN ""User"" u ON b.id_user = u.id_user
            WHERE b.is_paid != 'Оплачен'
              AND b.date_issued >= @from AND b.date_issued <= @to", connection);

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var item = new DebtReportItem
                    {
                        BillNumber = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Amount = reader.GetDouble(3)
                    };
                    report.Items.Add(item);
                    report.TotalDebt += item.Amount;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации отчета о задолженностях: {ex.Message}");
            }

            return report;
        }
        public static async Task<LawyerWorkReport> GenerateLawyerWorkReportAsync(NpgsqlConnection connection, DateTime from, DateTime to, int lawyerId = 0)
        {
            var report = new LawyerWorkReport
            {
                Period = $"{from:dd.MM.yyyy} - {to:dd.MM.yyyy}",
                SelectedLawyerId = lawyerId > 0 ? lawyerId : (int?)null
            };

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                var cmdText = @"
            SELECT 
                u.id_user,
                u.lastname || ' ' || u.firstname AS lawyer_name,
                COUNT(c.id_case) AS total_cases,
                SUM(CASE WHEN c.status = 'Создано' THEN 1 ELSE 0 END) AS created_cases,
                SUM(CASE WHEN c.status = 'В работе' THEN 1 ELSE 0 END) AS in_progress_cases,
                SUM(CASE WHEN c.status = 'Закрыто' THEN 1 ELSE 0 END) AS closed_cases
            FROM ""User"" u
            LEFT JOIN ""Case"" c ON u.id_user = c.id_lawyer
                AND c.data_add >= @from AND c.data_add <= @to
            WHERE u.role = 'lawyer'";

                if (lawyerId > 0)
                {
                    cmdText += " AND u.id_user = @lawyerId";
                }

                cmdText += @"
            GROUP BY u.id_user, lawyer_name
            ORDER BY total_cases DESC";

                using var cmd = new NpgsqlCommand(cmdText, connection);

                if (lawyerId > 0)
                {
                    cmd.Parameters.AddWithValue("@lawyerId", lawyerId);
                }

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var item = new LawyerWorkReportItem
                    {
                        LawyerId = reader.GetInt32(0),
                        LawyerName = reader.GetString(1),
                        TotalCases = reader.GetInt32(2),
                        CreatedCases = reader.GetInt32(3),
                        InProgressCases = reader.GetInt32(4),
                        ClosedCases = reader.GetInt32(5)
                    };

                    report.Items.Add(item);
                    report.TotalAllCases += item.TotalCases;
                    report.TotalCreated += item.CreatedCases;
                    report.TotalInProgress += item.InProgressCases;
                    report.TotalClosed += item.ClosedCases;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации отчета о работе юристов: {ex.Message}");
            }

            return report;
        }
    }
}