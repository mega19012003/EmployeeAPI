using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatedb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "CompanyName",
                table: "LogStatusConfigs");

            migrationBuilder.InsertData(
                table: "LogStatusConfigs",
                columns: new[] { "Id", "CompanyId", "IsSystemDefault", "Name", "Note", "SalaryMultiplier", "enumId" },
                values: new object[,]
                {
                    { new Guid("0c7b1683-c35b-4b1a-bd3a-a3cd7c797836"), null, true, "Late", "Đi trễ", 0.69999999999999996, 2 },
                    { new Guid("1944bac4-db1e-471c-a9ce-5dab3837ada0"), null, true, "OnHolidayOvertime", "Làm thêm giờ vào ngày nghỉ", 3.0, 11 },
                    { new Guid("21042925-eb77-4933-9645-069e0be412ac"), null, true, "OnTime", "Đi đúng giờ", 1.0, 1 },
                    { new Guid("2d646cf6-efd2-4907-8e37-a178752d7643"), null, true, "Absent", "Vắng", 0.0, 7 },
                    { new Guid("44b1036e-22cb-4acb-87f3-4e24ca2e4cda"), null, true, "Others", "Khác", 0.5, 14 },
                    { new Guid("554b8d8a-19b0-409f-ae09-3339e2ab466f"), null, true, "OnHoliday", "Làm vào ngày nghỉ", 2.0, 8 },
                    { new Guid("55ce8909-f89f-432f-8871-46e88bec8f32"), null, true, "OnHolidayLateAndOvertime", "Đi trễ và làm thêm giờ vào ngày nghỉ", 1.5, 12 },
                    { new Guid("55e7d8ad-9a02-40cc-9c2b-2d91db086ba4"), null, true, "Overtime", "Làm thêm giờ", 1.3, 5 },
                    { new Guid("5bc84931-0baf-4db6-96fd-9cfff0bbe072"), null, true, "OnHolidayLateAndLeaveEarly", "Đi trễ và về sớm vào ngày nghỉ", 2.0, 13 },
                    { new Guid("71868a4b-3ff3-412e-b93b-84af98894c25"), null, true, "LateAndOvertime", "Đi trễ và làm thêm giờ", 0.69999999999999996, 6 },
                    { new Guid("8521060d-3bdf-40f7-87eb-e31c2de87c67"), null, true, "LeaveEarly", "Về sớm", 0.69999999999999996, 3 },
                    { new Guid("96867600-6aa7-4884-b637-321b258f8b01"), null, true, "OnHolidayLeaveEarly", "Về sớm vào ngày nghỉ", 1.5, 10 },
                    { new Guid("a2c0d124-52dc-4a99-84eb-5cfe762d73f0"), null, true, "LateAndLeaveEarly", "Đi trễ và về sớm", 0.5, 4 },
                    { new Guid("af554b5a-72c7-49eb-9f96-fa915c88e575"), null, true, "OnHolidayLate", "Đi trễ vào ngày nghỉ", 1.5, 9 },
                    { new Guid("e698a08d-1998-4e15-a117-9a189abfd7d5"), null, true, "None", "Chưa checkin/checkout", 0.0, 0 }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTimes",
                columns: new[] { "id", "CompanyId", "EndTimeAfternoon", "EndTimeMorning", "IsSystemDefault", "LogAllowtime", "StartTimeAfternoon", "StartTimeMorning" },
                values: new object[] { new Guid("d8bb41fb-0d97-4062-b6ab-1bfee934c82f"), null, new TimeOnly(17, 0, 0), new TimeOnly(12, 0, 0), true, 5, new TimeOnly(13, 0, 0), new TimeOnly(8, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0c7b1683-c35b-4b1a-bd3a-a3cd7c797836"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("1944bac4-db1e-471c-a9ce-5dab3837ada0"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("21042925-eb77-4933-9645-069e0be412ac"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2d646cf6-efd2-4907-8e37-a178752d7643"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44b1036e-22cb-4acb-87f3-4e24ca2e4cda"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("554b8d8a-19b0-409f-ae09-3339e2ab466f"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("55ce8909-f89f-432f-8871-46e88bec8f32"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("55e7d8ad-9a02-40cc-9c2b-2d91db086ba4"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5bc84931-0baf-4db6-96fd-9cfff0bbe072"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("71868a4b-3ff3-412e-b93b-84af98894c25"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("8521060d-3bdf-40f7-87eb-e31c2de87c67"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("96867600-6aa7-4884-b637-321b258f8b01"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2c0d124-52dc-4a99-84eb-5cfe762d73f0"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("af554b5a-72c7-49eb-9f96-fa915c88e575"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e698a08d-1998-4e15-a117-9a189abfd7d5"));

            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "id",
                keyValue: new Guid("d8bb41fb-0d97-4062-b6ab-1bfee934c82f"));

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "LogStatusConfigs",
                type: "nvarchar(max)",
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
        }
    }
}
