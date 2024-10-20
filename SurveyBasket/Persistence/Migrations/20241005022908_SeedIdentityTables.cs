using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SurveyBasket.persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "46235578-03e2-43cf-9344-6a0c7b20925d", "d7aaa1d4-a150-4044-b84c-2c59417140f7", true, false, "Member", "MEMBER" },
                    { "aa6f1471-6662-4a6e-97b9-d27745585bda", "8c841d4d-5a09-42a9-b6ff-9d7f362f5477", false, false, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "7292e4ca-33b6-4b21-a314-7f72262391fd", 0, "334dcfc8-a525-4a0b-b92f-62c571f9c439", "admin@surveybasket.com", true, "Survey Basket", "Admin", false, null, "ADMIN@SURVEYBASKET.COM", "ADMIN@SURVEYBASKET.COM", "AQAAAAIAAYagAAAAEKoAE+QrT7YXycFY0U6UrEuCfxjfSUNIkpWu+y9mmwn3tIzDR9d27DvlAw4YNyOaQw==", null, false, "07DB2EDBB86447CA8B2EC4E293AE89F5", false, "admin@surveybasket.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "permission", "polls:read", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 2, "permission", "polls:add", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 3, "permission", "polls:update", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 4, "permission", "polls:delete", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 5, "permission", "questions:read", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 6, "permission", "questions:add", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 7, "permission", "questions:update", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 8, "permission", "users:read", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 9, "permission", "users:add", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 10, "permission", "users:update", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 11, "permission", "roles:read", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 12, "permission", "roles:add", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 13, "permission", "roles:update", "aa6f1471-6662-4a6e-97b9-d27745585bda" },
                    { 14, "permission", "results:read", "aa6f1471-6662-4a6e-97b9-d27745585bda" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "aa6f1471-6662-4a6e-97b9-d27745585bda", "7292e4ca-33b6-4b21-a314-7f72262391fd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "46235578-03e2-43cf-9344-6a0c7b20925d");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "aa6f1471-6662-4a6e-97b9-d27745585bda", "7292e4ca-33b6-4b21-a314-7f72262391fd" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "aa6f1471-6662-4a6e-97b9-d27745585bda");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7292e4ca-33b6-4b21-a314-7f72262391fd");
        }
    }
}
