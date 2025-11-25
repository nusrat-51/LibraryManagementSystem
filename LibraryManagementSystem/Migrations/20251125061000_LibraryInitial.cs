using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class LibraryInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueRecords",
                table: "IssueRecords");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "IssueRecords");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "IssueRecords");

            migrationBuilder.RenameColumn(
                name: "BookTitle",
                table: "IssueRecords",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "IssueRecords",
                newName: "BookId");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "IssueRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "IssueRecordId",
                table: "IssueRecords",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "BookId1",
                table: "IssueRecords",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "IssueRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueRecords",
                table: "IssueRecords",
                column: "IssueRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueRecords_BookId1",
                table: "IssueRecords",
                column: "BookId1");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueRecords_Books_BookId1",
                table: "IssueRecords",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueRecords_Books_BookId1",
                table: "IssueRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueRecords",
                table: "IssueRecords");

            migrationBuilder.DropIndex(
                name: "IX_IssueRecords_BookId1",
                table: "IssueRecords");

            migrationBuilder.DropColumn(
                name: "IssueRecordId",
                table: "IssueRecords");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "IssueRecords");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "IssueRecords");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "IssueRecords",
                newName: "BookTitle");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "IssueRecords",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "IssueRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "IssueRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "IssueRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueRecords",
                table: "IssueRecords",
                column: "Id");
        }
    }
}
