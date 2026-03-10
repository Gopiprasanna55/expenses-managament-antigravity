using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace ExportScript
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbPath = @"..\ExpenseManager.Web\ExpenseManager.db";
            
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Date, Amount, Title, Description 
                    FROM Expenses 
                    WHERE CompanyId = '' 
                    ORDER BY Date DESC";

                var lines = new List<string> { "Date,Amount,Title,Description" };
                decimal total = 0;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = reader.GetString(0);
                        var amount = reader.GetDecimal(1);
                        var title = reader.IsDBNull(2) ? "" : reader.GetString(2).Replace("\"", "\"\"");
                        var desc = reader.IsDBNull(3) ? "" : reader.GetString(3).Replace("\"", "\"\"");

                        lines.Add($"{date},{amount},\"{title}\",\"{desc}\"");
                        total += amount;
                    }
                }

                lines.Add($",,,");
                lines.Add($"TOTAL,{total},,");
                
                File.WriteAllLines("AllExpensesDump.csv", lines);
                Console.WriteLine($"Exported expenses to AllExpensesDump.csv. Total: {total}");
            }
        }
    }
}
