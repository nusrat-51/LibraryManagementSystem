using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookApplications");

            migrationBuilder.AddColumn<string>(
                name: "StudentEmail",
                table: "BookReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BookReturns_BookId",
                table: "BookReturns",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReturns_Books_BookId",
                table: "BookReturns",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReturns_Books_BookId",
                table: "BookReturns");

            migrationBuilder.DropIndex(
                name: "IX_BookReturns_BookId",
                table: "BookReturns");

            migrationBuilder.DropColumn(
                name: "StudentEmail",
                table: "BookReturns");

            migrationBuilder.CreateTable(
                name: "BookApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MemberId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookApplications_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookApplications_BookId",
                table: "BookApplications",
                column: "BookId");
        }
    }
}
