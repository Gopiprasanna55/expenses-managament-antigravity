import sqlite3
import json

db_path = "ExpenseManager.db"
try:
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Get users
    cursor.execute("SELECT Id, Email, CompanyId FROM AspNetUsers;")
    users = cursor.fetchall()
    
    # Get categories
    cursor.execute("SELECT Id, Name, CompanyId FROM Categories;")
    categories = cursor.fetchall()

    print("Users:")
    for u in users:
        print(u)
        
    print("\nCategories:")
    for c in categories:
        print(c)
        
    conn.close()
except Exception as e:
    print(f"Error: {e}")
