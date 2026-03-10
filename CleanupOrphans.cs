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
    
    Console.WriteLine("Starting cleanup of orphaned WalletActivity records...");
    
    var activities = await context.WalletActivities
        .Where(a => a.Type == WalletActivityType.Expense)
        .ToListAsync();
        
    int deletedCount = 0;
    int linkedCount = 0;
    
    foreach (var activity in activities)
    {
        // If it's already linked, skip
        if (activity.ExpenseId.HasValue) continue;
        
        // Try to find a matching expense
        var matchingExpense = await context.Expenses
            .FirstOrDefaultAsync(e => 
                e.Amount == activity.Amount && 
                e.Date.Date == activity.Date.Date && 
                e.UserId == activity.UserId && 
                e.CompanyId == activity.CompanyId);
                
        if (matchingExpense != null)
        {
            activity.ExpenseId = matchingExpense.Id;
            linkedCount++;
        }
        else
        {
            context.WalletActivities.Remove(activity);
            deletedCount++;
        }
    }
    
    await context.SaveChangesAsync();
    Console.WriteLine($"Cleanup complete. Linked: {linkedCount}, Deleted (Orphaned): {deletedCount}");
}
