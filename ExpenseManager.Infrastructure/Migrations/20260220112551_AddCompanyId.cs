using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "Wallets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "WalletActivities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "Expenses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "Categories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CompanyId",
                table: "Wallets",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletActivities_CompanyId",
                table: "WalletActivities",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CompanyId",
                table: "Expenses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CompanyId",
                table: "Categories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_CompanyId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_WalletActivities_CompanyId",
                table: "WalletActivities");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_CompanyId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CompanyId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "WalletActivities");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AspNetUsers");
        }
    }
}
