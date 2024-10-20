using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyBasket.persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDisabledColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7292e4ca-33b6-4b21-a314-7f72262391fd",
                columns: new[] { "IsDisabled", "PasswordHash" },
                values: new object[] { false, "AQAAAAIAAYagAAAAENaKq+dbHjvM5g4sMe7tlQb4xnoPUagEK17q3frKbF92KmaUaQPRkSGqOzyMH07pww==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7292e4ca-33b6-4b21-a314-7f72262391fd",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAECQyqw3sUR9VXGpTWQPxS5dn1dzSstxNPhnn+F+VgCMQlGnJsbwIXu3AlSUwnwPyNw==");
        }
    }
}
