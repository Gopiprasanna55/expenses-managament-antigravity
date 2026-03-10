using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using ExpenseManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=ExpenseManager.Web/ExpenseManager.db"));
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    Console.WriteLine("Fetching all expenses for CompanyId ''...");
    
    var expenses = await context.Expenses
        .Where(e => e.CompanyId == "")
        .OrderByDescending(e => e.Date)
        .ToListAsync();
        
    var lines = new System.Collections.Generic.List<string> { "Date,Amount,Title,Description" };
    decimal total = 0;
    
    foreach(var e in expenses)
    {
        lines.Add($"{e.Date:yyyy-MM-dd},{e.Amount},\"{e.Title}\",\"{e.Description}\"");
        total += e.Amount;
    }
    
    lines.Add($",,,");
    lines.Add($"TOTAL,{total},,");
    
    File.WriteAllLines("AllExpensesDump.csv", lines);
    Console.WriteLine($"Exported {expenses.Count} expenses to AllExpensesDump.csv. Total: {total}");
}
