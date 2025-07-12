using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class themuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isPaied",
                table: "Payrolls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "CompanyId", "DepartmentId", "Fullname", "ImageUrl", "IsActive", "IsDeleted", "Password", "PhoneNumber", "PositionId", "RefreshToken", "RefreshTokenExpiryTime", "Role", "SalaryPerHour", "TokenVersion", "Username" },
                values: new object[,]
                {
                    { new Guid("152738ba-4a3f-4fc4-ac33-b39a8cbe71a6"), "Đà Nẵng", null, null, "Nguyễn Phúc Bảo", "", true, false, "tHQAWj+qap6c3KwjGXhVowUhogBYjGpZFYZbrEnFqgZWFA4d", "0901000004", null, "", null, 3, 15000.0, 0, "Employee03" },
                    { new Guid("4baf844c-b009-4dc7-b41b-d40bd6516bc0"), "Hải Phòng", null, null, "Phạm Văn An", "", true, false, "E0Yn2qgiHJw2k6tok1y9Krg3x0U96QDG7L6MN6JsYBrfu3K4", "0901000003", null, "", null, 3, 12000.0, 0, "Employee02" },
                    { new Guid("4bb4af3b-d172-4108-a7d3-73257cbea84c"), "Hà Nội", null, null, "Nguyễn Văn Quang", "", true, false, "DiMXpjS7sJ+r9qrrIpnly64KZ8QZqtMqs9G/8grgq2AxAk0e", "0901000001", null, "", null, 2, 20000.0, 0, "Manager01" },
                    { new Guid("5967e51c-4a4e-4d4b-b9ac-603a5c0dcdf4"), "Huế", null, null, "Trần Minh Đức", "", true, false, "TH14IuYAmLEpz8z9HTg70lAnDChwvwVTgqri+d6U/tYcpin1", "0901000005", null, "", null, 3, 8000.0, 0, "Employee04" },
                    { new Guid("641771ad-dde6-437d-9c4a-38cf7064630a"), "Hà Nội", null, null, "Lê Bảo Nhân", "", true, false, "Jl6AjwMfoXtLqTWVDpNGHvK8AhLQK/GUcnxay7Ovh2m8JNTG", "0901000010", null, "", null, 2, 15000.0, 0, "Manager04" },
                    { new Guid("7b021be6-c477-4dd4-89dd-67e7916a7b0d"), "Hà Nội", null, null, "Nguyễn Phúc Hậu", "", true, false, "v0UtYeysX9i7BubBElkarwDzXtmC3wmnUKIssAXtZmpAA9B1", "0901000009", null, "", null, 2, 18000.0, 0, "Manager03" },
                    { new Guid("846692d4-d919-4b49-915e-7724824c35de"), "Ninh Bình", null, null, "Trần Minh Quân", "", true, false, "HxfTkSNfSkBfVQVn9B013R7tbD8Sk8SVd/+Wt9jpXtec3SC5", "0901000008", null, "", null, 3, 10000.0, 0, "User01" },
                    { new Guid("ce8b19d6-fbd2-4781-b0a0-a2fce49c6b37"), "TP. HCM", null, null, "Lê Thị Hoa", "", true, false, "sHDbOMkHJWoH5kd8w2w4c6d7+E0ozPuCwmk1O40L1zJjkvl6", "0901000002", null, "", null, 2, 20000.0, 0, "Manager02" },
                    { new Guid("d4f2a1bd-8a24-456a-a8a3-c1bb03f2fa78"), "Bình Dương", null, null, "Vũ Thị Ngọc Bích", "", true, false, "ircMRUaJ5uGQpUsdz0x42Mk6BVnUd+GpHK+taIlQHBAa7gLr", "0901000007", null, "", null, 3, 12000.0, 0, "Employee06" },
                    { new Guid("e72839f3-209b-4449-a240-e69b7abe8d52"), "Cần Thơ", null, null, "Lê Văn Dũng", "", true, false, "sH4PFnyOU2Om9xBEvgD0jhC69v9n2gN3Vbtei+C4orm/L64e", "0901000006", null, "", null, 3, 5000.0, 0, "Employee05" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("152738ba-4a3f-4fc4-ac33-b39a8cbe71a6"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("4baf844c-b009-4dc7-b41b-d40bd6516bc0"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("4bb4af3b-d172-4108-a7d3-73257cbea84c"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("5967e51c-4a4e-4d4b-b9ac-603a5c0dcdf4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("641771ad-dde6-437d-9c4a-38cf7064630a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("7b021be6-c477-4dd4-89dd-67e7916a7b0d"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("846692d4-d919-4b49-915e-7724824c35de"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("ce8b19d6-fbd2-4781-b0a0-a2fce49c6b37"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("d4f2a1bd-8a24-456a-a8a3-c1bb03f2fa78"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("e72839f3-209b-4449-a240-e69b7abe8d52"));

            migrationBuilder.DropColumn(
                name: "isPaied",
                table: "Payrolls");
        }
    }
}
