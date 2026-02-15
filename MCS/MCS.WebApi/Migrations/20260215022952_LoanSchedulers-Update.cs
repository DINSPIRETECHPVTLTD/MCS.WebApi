using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCS.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class LoanSchedulersUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanPayments");

            migrationBuilder.AddColumn<int>(
                name: "CollectedBy",
                table: "LoanSchedulers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "LoanSchedulers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentNo",
                table: "LoanSchedulers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestAmount",
                table: "LoanSchedulers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LoanId1",
                table: "LoanSchedulers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMode",
                table: "LoanSchedulers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrincipalAmount",
                table: "LoanSchedulers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SavingAmount",
                table: "LoanSchedulers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LoanSchedulers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CollectionTerm",
                table: "Loans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NoOfTerms",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LoanSchedulers_CollectedBy",
                table: "LoanSchedulers",
                column: "CollectedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LoanSchedulers_LoanId1",
                table: "LoanSchedulers",
                column: "LoanId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanSchedulers_Loans_LoanId1",
                table: "LoanSchedulers",
                column: "LoanId1",
                principalTable: "Loans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanSchedulers_Users_CollectedBy",
                table: "LoanSchedulers",
                column: "CollectedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanSchedulers_Loans_LoanId1",
                table: "LoanSchedulers");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanSchedulers_Users_CollectedBy",
                table: "LoanSchedulers");

            migrationBuilder.DropIndex(
                name: "IX_LoanSchedulers_CollectedBy",
                table: "LoanSchedulers");

            migrationBuilder.DropIndex(
                name: "IX_LoanSchedulers_LoanId1",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "CollectedBy",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "InstallmentNo",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "InterestAmount",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "LoanId1",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "PrincipalAmount",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "SavingAmount",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LoanSchedulers");

            migrationBuilder.DropColumn(
                name: "CollectionTerm",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "NoOfTerms",
                table: "Loans");

            migrationBuilder.CreateTable(
                name: "LoanPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ActualPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstallmentNo = table.Column<int>(type: "int", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SavingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanPayments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanPayments_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanPayments_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_CreatedBy",
                table: "LoanPayments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_LoanId",
                table: "LoanPayments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_ModifiedBy",
                table: "LoanPayments",
                column: "ModifiedBy");
        }
    }
}
