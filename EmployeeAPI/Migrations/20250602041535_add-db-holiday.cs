using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class adddbholiday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Id", "IsDeleted", "endDate", "name", "startDate" },
                values: new object[,]
                {
                    { new Guid("0bf85d7e-bd1e-46f8-9836-fe7e368c7384"), false, new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ngày Quốc tế Lao động", new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("6d2e6a69-7374-4f5c-9100-a12361ceb258"), false, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Giỗ Tổ Hùng Vương", new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("91a83653-f975-461f-b6a7-a5645230e094"), false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tết Dương lịch", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b3c06886-2483-409f-980e-eda736dea5b4"), false, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tết Nguyên đán", new DateTime(2025, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("c77d0b38-7f41-4f0c-942b-54cd44ed99f1"), false, new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ngày Giải phóng miền Nam", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("d87d91ee-44de-489d-a8df-6ceeb890e242"), false, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ngày Quốc khánh", new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("0bf85d7e-bd1e-46f8-9836-fe7e368c7384"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("6d2e6a69-7374-4f5c-9100-a12361ceb258"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("91a83653-f975-461f-b6a7-a5645230e094"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("b3c06886-2483-409f-980e-eda736dea5b4"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("c77d0b38-7f41-4f0c-942b-54cd44ed99f1"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("d87d91ee-44de-489d-a8df-6ceeb890e242"));
        }
    }
}
