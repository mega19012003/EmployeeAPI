using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class suamodelcheckin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Checkins",
                newName: "updateBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Checkins",
                newName: "UpdateAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updateBy",
                table: "Checkins",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdateAt",
                table: "Checkins",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Checkins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
