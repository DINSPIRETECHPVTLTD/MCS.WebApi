using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCS.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class LoanSchedulersUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CollectedBy",
                table: "LoanSchedulers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualEmiAmount",
                table: "LoanSchedulers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualInterestAmount",
                table: "LoanSchedulers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualPrincipalAmount",
                table: "LoanSchedulers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualEmiAmount",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "ActualInterestAmount",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "ActualPrincipalAmount",
                table: "LoanSchedulers");

            migrationBuilder.AlterColumn<int>(
                name: "CollectedBy",
                table: "LoanSchedulers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
