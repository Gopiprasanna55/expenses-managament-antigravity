using System;
using System.Linq;
using System.Threading.Tasks;
using ExpenseManager.Infrastructure.Data;
using ExpenseManager.Domain.Enums;
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
    
    Console.WriteLine("Dumping all WalletActivity records:");
    var activities = await context.WalletActivities.ToListAsync();
    
    foreach (var a in activities)
    {
        Console.WriteLine($"ID: {a.Id}, CompanyId: '{a.CompanyId}', Type: {a.Type}, Amount: {a.Amount}, ExpenseId: {a.ExpenseId}, Date: {a.Date}");
    }
    
    Console.WriteLine("\nDumping all Expense records:");
    var expenses = await context.Expenses.ToListAsync();
    foreach (var e in expenses)
    {
        Console.WriteLine($"ID: {e.Id}, CompanyId: '{e.CompanyId}', Amount: {e.Amount}, Title: {e.Title}");
    }
}
