using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBookApplicationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookApplications_BookId",
                table: "BookApplications");

            migrationBuilder.AddColumn<string>(
                name: "StudentEmail",
                table: "BookApplications",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BookApplications_BookId_StudentEmail",
                table: "BookApplications",
                columns: new[] { "BookId", "StudentEmail" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookApplications_BookId_StudentEmail",
                table: "BookApplications");

            migrationBuilder.DropColumn(
                name: "StudentEmail",
                table: "BookApplications");

            migrationBuilder.CreateIndex(
                name: "IX_BookApplications_BookId",
                table: "BookApplications",
                column: "BookId");
        }
    }
}
