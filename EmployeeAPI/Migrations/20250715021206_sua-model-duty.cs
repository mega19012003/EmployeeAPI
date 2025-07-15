using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class suamodelduty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("08eafcc1-dc50-4c78-8cbe-7d07a760b3b7"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0c705b2a-9d65-4f53-9899-600b679459f6"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("149c3474-8834-416a-a025-b4239a5146a3"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("74d5dd05-15f0-47cc-847f-f9daa1f05b0f"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7b0d6f5a-ac95-45e2-b38d-7c3a51c6ab98"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("871473b1-5528-4870-be86-fa2b8afbad79"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("8b08faba-35d3-4951-9db9-10bbcac4cfea"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a356d277-d902-4ee4-abea-209475628832"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a92a985a-d572-4e0c-aaf2-82d088d53deb"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b450d4b9-e3d1-4463-ba19-a715fbcc700b"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b71749b1-0575-42cb-9016-3394d2e9844f"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ce6a7402-d9a1-416e-a0de-06b3738d0e70"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("d4389218-f965-4803-a87a-d3c9cbf3ba8c"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("da365ea0-63d5-43c1-9041-9ca1035c2e0c"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f1be3ddd-e335-408c-bd70-c06586140130"));

            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "id",
                keyValue: new Guid("bf10b6e0-aefa-4e6e-989a-a2fc7ec12890"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Duties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "LogStatusConfigs",
                columns: new[] { "Id", "CompanyId", "CompanyName", "IsSystemDefault", "Name", "Note", "SalaryMultiplier", "enumId" },
                values: new object[,]
                {
                    { new Guid("0a52e304-8612-4f9e-975b-8fe6ed39fb11"), null, null, true, "OnTime", "Đi đúng giờ", 1.0, 1 },
                    { new Guid("21d85fb7-ca24-404a-9ca4-d28088f4d02d"), null, null, true, "OnHolidayLeaveEarly", "Về sớm vào ngày nghỉ", 1.5, 10 },
                    { new Guid("2ac46cba-5352-46fd-bed1-ebfe7fb69767"), null, null, true, "None", "Chưa checkin/checkout", 0.0, 0 },
                    { new Guid("3026af5f-3294-48f5-976e-63b572d294c6"), null, null, true, "LateAndOvertime", "Đi trễ và làm thêm giờ", 0.69999999999999996, 6 },
                    { new Guid("4bc00383-4cdb-4ea4-ab70-72f78083d3b8"), null, null, true, "LeaveEarly", "Về sớm", 0.69999999999999996, 3 },
                    { new Guid("61601d14-7431-4c53-885e-cd666eec7681"), null, null, true, "Absent", "Vắng", 0.0, 7 },
                    { new Guid("67d1c6d5-d1b8-4e5f-8c9b-e9436a3a664b"), null, null, true, "Others", "Khác", 0.5, 14 },
                    { new Guid("6bacf0ed-cce8-4b64-a493-0a726e9ff385"), null, null, true, "OnHoliday", "Làm vào ngày nghỉ", 2.0, 8 },
                    { new Guid("8197a982-25a2-457f-bb4e-cf01b8ff48d5"), null, null, true, "Late", "Đi trễ", 0.69999999999999996, 2 },
                    { new Guid("855426f4-734d-4c65-8f63-b5c3f3613c39"), null, null, true, "OnHolidayLateAndOvertime", "Đi trễ và làm thêm giờ vào ngày nghỉ", 1.5, 12 },
                    { new Guid("99f42b79-1561-45c2-8bcc-80ac88a3d0e8"), null, null, true, "OnHolidayOvertime", "Làm thêm giờ vào ngày nghỉ", 3.0, 11 },
                    { new Guid("9f9c4755-50d7-48b7-8bfb-6c651bb28a37"), null, null, true, "Overtime", "Làm thêm giờ", 1.3, 5 },
                    { new Guid("afce81bc-0f42-45c8-9952-835dbc867bec"), null, null, true, "OnHolidayLateAndLeaveEarly", "Đi trễ và về sớm vào ngày nghỉ", 2.0, 13 },
                    { new Guid("ca810601-dde3-4002-99e0-a107bc223a56"), null, null, true, "OnHolidayLate", "Đi trễ vào ngày nghỉ", 1.5, 9 },
                    { new Guid("f4019970-1ed9-423b-97dc-9a6e50eb380f"), null, null, true, "LateAndLeaveEarly", "Đi trễ và về sớm", 0.5, 4 }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTimes",
                columns: new[] { "id", "CompanyId", "EndTimeAfternoon", "EndTimeMorning", "IsSystemDefault", "LogAllowtime", "StartTimeAfternoon", "StartTimeMorning" },
                values: new object[] { new Guid("2a5a5a36-cbef-4e9e-a769-976bc1883179"), null, new TimeOnly(17, 0, 0), new TimeOnly(12, 0, 0), true, 5, new TimeOnly(13, 0, 0), new TimeOnly(8, 0, 0) });

            migrationBuilder.CreateIndex(
                name: "IX_Duties_CompanyId",
                table: "Duties",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Duties_Companies_CompanyId",
                table: "Duties",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Duties_Companies_CompanyId",
                table: "Duties");

            migrationBuilder.DropIndex(
                name: "IX_Duties_CompanyId",
                table: "Duties");

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a52e304-8612-4f9e-975b-8fe6ed39fb11"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("21d85fb7-ca24-404a-9ca4-d28088f4d02d"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2ac46cba-5352-46fd-bed1-ebfe7fb69767"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("3026af5f-3294-48f5-976e-63b572d294c6"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("4bc00383-4cdb-4ea4-ab70-72f78083d3b8"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("61601d14-7431-4c53-885e-cd666eec7681"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("67d1c6d5-d1b8-4e5f-8c9b-e9436a3a664b"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("6bacf0ed-cce8-4b64-a493-0a726e9ff385"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("8197a982-25a2-457f-bb4e-cf01b8ff48d5"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("855426f4-734d-4c65-8f63-b5c3f3613c39"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("99f42b79-1561-45c2-8bcc-80ac88a3d0e8"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9f9c4755-50d7-48b7-8bfb-6c651bb28a37"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("afce81bc-0f42-45c8-9952-835dbc867bec"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ca810601-dde3-4002-99e0-a107bc223a56"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f4019970-1ed9-423b-97dc-9a6e50eb380f"));

            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "id",
                keyValue: new Guid("2a5a5a36-cbef-4e9e-a769-976bc1883179"));

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Duties");

            migrationBuilder.InsertData(
                table: "LogStatusConfigs",
                columns: new[] { "Id", "CompanyId", "CompanyName", "IsSystemDefault", "Name", "Note", "SalaryMultiplier", "enumId" },
                values: new object[,]
                {
                    { new Guid("08eafcc1-dc50-4c78-8cbe-7d07a760b3b7"), null, null, true, "Late", "Đi trễ", 0.69999999999999996, 2 },
                    { new Guid("0c705b2a-9d65-4f53-9899-600b679459f6"), null, null, true, "Absent", "Vắng", 0.0, 7 },
                    { new Guid("149c3474-8834-416a-a025-b4239a5146a3"), null, null, true, "OnHolidayLateAndOvertime", "Đi trễ và làm thêm giờ vào ngày nghỉ", 1.5, 12 },
                    { new Guid("74d5dd05-15f0-47cc-847f-f9daa1f05b0f"), null, null, true, "OnTime", "Đi đúng giờ", 1.0, 1 },
                    { new Guid("7b0d6f5a-ac95-45e2-b38d-7c3a51c6ab98"), null, null, true, "LateAndOvertime", "Đi trễ và làm thêm giờ", 0.69999999999999996, 6 },
                    { new Guid("871473b1-5528-4870-be86-fa2b8afbad79"), null, null, true, "Overtime", "Làm thêm giờ", 1.3, 5 },
                    { new Guid("8b08faba-35d3-4951-9db9-10bbcac4cfea"), null, null, true, "LateAndLeaveEarly", "Đi trễ và về sớm", 0.5, 4 },
                    { new Guid("a356d277-d902-4ee4-abea-209475628832"), null, null, true, "None", "Chưa checkin/checkout", 0.0, 0 },
                    { new Guid("a92a985a-d572-4e0c-aaf2-82d088d53deb"), null, null, true, "OnHolidayLate", "Đi trễ vào ngày nghỉ", 1.5, 9 },
                    { new Guid("b450d4b9-e3d1-4463-ba19-a715fbcc700b"), null, null, true, "OnHolidayLateAndLeaveEarly", "Đi trễ và về sớm vào ngày nghỉ", 2.0, 13 },
                    { new Guid("b71749b1-0575-42cb-9016-3394d2e9844f"), null, null, false, "Others", "Khác", 0.5, 14 },
                    { new Guid("ce6a7402-d9a1-416e-a0de-06b3738d0e70"), null, null, true, "OnHoliday", "Làm vào ngày nghỉ", 2.0, 8 },
                    { new Guid("d4389218-f965-4803-a87a-d3c9cbf3ba8c"), null, null, true, "OnHolidayOvertime", "Làm thêm giờ vào ngày nghỉ", 3.0, 11 },
                    { new Guid("da365ea0-63d5-43c1-9041-9ca1035c2e0c"), null, null, true, "LeaveEarly", "Về sớm", 0.69999999999999996, 3 },
                    { new Guid("f1be3ddd-e335-408c-bd70-c06586140130"), null, null, true, "OnHolidayLeaveEarly", "Về sớm vào ngày nghỉ", 1.5, 10 }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTimes",
                columns: new[] { "id", "CompanyId", "EndTimeAfternoon", "EndTimeMorning", "IsSystemDefault", "LogAllowtime", "StartTimeAfternoon", "StartTimeMorning" },
                values: new object[] { new Guid("bf10b6e0-aefa-4e6e-989a-a2fc7ec12890"), null, new TimeOnly(17, 0, 0), new TimeOnly(12, 0, 0), true, 5, new TimeOnly(13, 0, 0), new TimeOnly(8, 0, 0) });
        }
    }
}
