using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScottNeidig.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_CreatedUtc",
                table: "ErrorLogs",
                column: "CreatedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogs");
        }
    }
}
