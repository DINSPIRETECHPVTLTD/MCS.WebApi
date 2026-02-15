using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCS.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoanCode",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "OutstandingAmount",
                table: "Loans");

            migrationBuilder.AddColumn<DateTime>(
                name: "CollectionStartDate",
                table: "Loans",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectionStartDate",
                table: "Loans");

            migrationBuilder.AddColumn<string>(
                name: "LoanCode",
                table: "Loans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OutstandingAmount",
                table: "Loans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
