using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class addiptable3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "AllowedIPs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "AllowedIPs");
        }
    }
}
