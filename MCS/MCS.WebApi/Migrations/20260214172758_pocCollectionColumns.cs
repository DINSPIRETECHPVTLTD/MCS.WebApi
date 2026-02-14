using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCS.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class pocCollectionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionBy",
                table: "POCs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CollectionDay",
                table: "POCs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollectionFrequency",
                table: "POCs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_POCs_CollectionBy",
                table: "POCs",
                column: "CollectionBy");

            migrationBuilder.AddForeignKey(
                name: "FK_POCs_Users_CollectionBy",
                table: "POCs",
                column: "CollectionBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POCs_Users_CollectionBy",
                table: "POCs");

            migrationBuilder.DropIndex(
                name: "IX_POCs_CollectionBy",
                table: "POCs");

            migrationBuilder.DropColumn(
                name: "CollectionBy",
                table: "POCs");

            migrationBuilder.DropColumn(
                name: "CollectionDay",
                table: "POCs");

            migrationBuilder.DropColumn(
                name: "CollectionFrequency",
                table: "POCs");
        }
    }
}
