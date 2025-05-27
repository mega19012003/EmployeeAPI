using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class ipdatepayroll1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_CheckinStatusConfigs_SalaryRuleId",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_SalaryRuleId",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "SalaryRuleId",
                table: "Payrolls");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalaryRuleId",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_SalaryRuleId",
                table: "Payrolls",
                column: "SalaryRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_CheckinStatusConfigs_SalaryRuleId",
                table: "Payrolls",
                column: "SalaryRuleId",
                principalTable: "CheckinStatusConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
