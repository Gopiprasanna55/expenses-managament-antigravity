import sqlite3

db_path = "ExpenseManager.db"
try:
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Sum of Expenses table
    cursor.execute("SELECT SUM(Amount) FROM Expenses;")
    expenses_sum = cursor.fetchone()[0] or 0

    # Sum of Expense records in WalletActivities table
    cursor.execute("SELECT SUM(Amount) FROM WalletActivities WHERE Type = 1;") # Assuming 1 is Expense
    wallet_activities_expense_sum = cursor.fetchone()[0] or 0
    
    # Sum of Recharge records in WalletActivities table
    cursor.execute("SELECT SUM(Amount) FROM WalletActivities WHERE Type = 0;") # Assuming 0 is Recharge
    wallet_activities_recharge_sum = cursor.fetchone()[0] or 0

    # Check for orphaned wallet activities
    cursor.execute("SELECT COUNT(*) FROM WalletActivities WHERE Type = 1 AND (ExpenseId IS NULL OR ExpenseId NOT IN (SELECT Id FROM Expenses));")
    orphaned_count = cursor.fetchone()[0]
    
    # Sum of orphaned wallet activities
    cursor.execute("SELECT SUM(Amount) FROM WalletActivities WHERE Type = 1 AND (ExpenseId IS NULL OR ExpenseId NOT IN (SELECT Id FROM Expenses));")
    orphaned_sum = cursor.fetchone()[0] or 0

    print(f"Total in Expenses Table: {expenses_sum}")
    print(f"Total Expenses in WalletActivities: {wallet_activities_expense_sum}")
    print(f"Total Recharges in WalletActivities: {wallet_activities_recharge_sum}")
    print(f"Orphaned Expense records in WalletActivities: count={orphaned_count}, sum={orphaned_sum}")
        
    conn.close()
except Exception as e:
    print(f"Error: {e}")
