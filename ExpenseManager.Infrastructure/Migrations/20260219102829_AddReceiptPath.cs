using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptPath",
                table: "Expenses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptPath",
                table: "Expenses");
        }
    }
}
