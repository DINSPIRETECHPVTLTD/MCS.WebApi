using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCS.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class LedgerTransaction2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerTransactions_Users_FromUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerTransactions_Users_ToUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_LedgerTransactions_FromUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_LedgerTransactions_ToUserId",
                table: "LedgerTransactions");

            migrationBuilder.AddColumn<int>(
                name: "PaidFromUserId",
                table: "LedgerTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaidToUserId",
                table: "LedgerTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransactions_PaidFromUserId",
                table: "LedgerTransactions",
                column: "PaidFromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransactions_PaidToUserId",
                table: "LedgerTransactions",
                column: "PaidToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerTransactions_Users_PaidFromUserId",
                table: "LedgerTransactions",
                column: "PaidFromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerTransactions_Users_PaidToUserId",
                table: "LedgerTransactions",
                column: "PaidToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerTransactions_Users_PaidFromUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerTransactions_Users_PaidToUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_LedgerTransactions_PaidFromUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_LedgerTransactions_PaidToUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropColumn(
                name: "PaidFromUserId",
                table: "LedgerTransactions");

            migrationBuilder.DropColumn(
                name: "PaidToUserId",
                table: "LedgerTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransactions_FromUserId",
                table: "LedgerTransactions",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransactions_ToUserId",
                table: "LedgerTransactions",
                column: "ToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerTransactions_Users_FromUserId",
                table: "LedgerTransactions",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerTransactions_Users_ToUserId",
                table: "LedgerTransactions",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
