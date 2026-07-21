using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScottNeidig.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryServiceKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceKey",
                table: "Categories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ServiceKey",
                table: "Categories",
                column: "ServiceKey",
                unique: true,
                filter: "[ServiceKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ServiceKey",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ServiceKey",
                table: "Categories");
        }
    }
}
