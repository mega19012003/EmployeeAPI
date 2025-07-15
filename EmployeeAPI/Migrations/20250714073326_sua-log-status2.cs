using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class sualogstatus2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
              name: "AllowedIPs",
              columns: table => new
              {
                  AllowedIPId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                  IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_AllowedIPs", x => x.AllowedIPId);
                  table.ForeignKey(
                      name: "FK_AllowedIPs_Companies_CompanyId",
                      column: x => x.CompanyId,
                      principalTable: "Companies",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
              });
            migrationBuilder.CreateIndex(
                  name: "IX_AllowedIPs_CompanyId",
                  table: "AllowedIPs",
                  column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
name: "AllowedIPs");
        }
    }
}
