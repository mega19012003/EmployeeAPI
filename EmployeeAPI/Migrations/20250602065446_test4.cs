using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class test4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("74e85ea7-09e0-44ce-95c2-c584a41d3d37"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("9e89e439-124a-4fb5-a85b-116dc4fbf821"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("a90bb4b5-0ba7-44ea-9b8e-eb39ca6a33d5"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("c38a26ef-d11f-477e-8c0c-90e80bd3618a"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("d1eced4c-8979-4159-b1b3-2ff9e796c1a1"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("fe7b47c5-31f5-4826-989b-00ce9579b70c"));*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Id", "IsDeleted", "endDate", "name", "startDate" },
                values: new object[,]
                {
                    { new Guid("74e85ea7-09e0-44ce-95c2-c584a41d3d37"), false, new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ngày Giải phóng miền Nam", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("9e89e439-124a-4fb5-a85b-116dc4fbf821"), false, new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ngày Quốc tế Lao động", new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("a90bb4b5-0ba7-44ea-9b8e-eb39ca6a33d5"), false, new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ngày Quốc khánh", new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("c38a26ef-d11f-477e-8c0c-90e80bd3618a"), false, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Giỗ Tổ Hùng Vương", new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("d1eced4c-8979-4159-b1b3-2ff9e796c1a1"), false, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tết Nguyên đán", new DateTime(2025, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("fe7b47c5-31f5-4826-989b-00ce9579b70c"), false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tết Dương lịch", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });*/
        }
    }
}
