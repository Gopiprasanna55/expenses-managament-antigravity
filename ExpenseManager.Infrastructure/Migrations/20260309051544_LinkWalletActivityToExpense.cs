using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkWalletActivityToExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "WalletActivities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletActivities_ExpenseId",
                table: "WalletActivities",
                column: "ExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletActivities_Expenses_ExpenseId",
                table: "WalletActivities",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletActivities_Expenses_ExpenseId",
                table: "WalletActivities");

            migrationBuilder.DropIndex(
                name: "IX_WalletActivities_ExpenseId",
                table: "WalletActivities");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "WalletActivities");
        }
    }
}
