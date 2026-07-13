using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrafficRules.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "b50a741a-0cfe-4196-aa42-5a759347a851", null, "Admin", "ADMIN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b50a741a-0cfe-4196-aa42-5a759347a851");
        }
    }
}
