using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentAnalyzer.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedByUserId",
                table: "Documents",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "Documents",
                newName: "Content");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Documents",
                newName: "UploadedByUserId");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Documents",
                newName: "FileType");
        }
    }
}
