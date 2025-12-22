using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddFineAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_IssueRecords_IssueRecordId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_BookApplications_BookId_StudentEmail",
                table: "BookApplications");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Fines");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Fines");

            migrationBuilder.RenameColumn(
                name: "PaidOn",
                table: "Fines",
                newName: "PaidAt");

            migrationBuilder.AlterColumn<int>(
                name: "IssueRecordId",
                table: "Fines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Fines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Fines",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentEmail",
                table: "BookApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FineId = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Fines_FineId",
                        column: x => x.FineId,
                        principalTable: "Fines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookApplications_BookId",
                table: "BookApplications",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FineId",
                table: "Payments",
                column: "FineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_IssueRecords_IssueRecordId",
                table: "Fines",
                column: "IssueRecordId",
                principalTable: "IssueRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_IssueRecords_IssueRecordId",
                table: "Fines");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_BookApplications_BookId",
                table: "BookApplications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Fines");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Fines");

            migrationBuilder.RenameColumn(
                name: "PaidAt",
                table: "Fines",
                newName: "PaidOn");

            migrationBuilder.AlterColumn<int>(
                name: "IssueRecordId",
                table: "Fines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Fines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Fines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentEmail",
                table: "BookApplications",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_BookApplications_BookId_StudentEmail",
                table: "BookApplications",
                columns: new[] { "BookId", "StudentEmail" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_IssueRecords_IssueRecordId",
                table: "Fines",
                column: "IssueRecordId",
                principalTable: "IssueRecords",
                principalColumn: "Id");
        }
    }
}
