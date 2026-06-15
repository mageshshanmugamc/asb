using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASB.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "DisplayOrder", "Icon", "Name", "ParentMenuId", "Route" },
                values: new object[] { 15, 4, "public", "Countries", 4, "/settings/country" });

            migrationBuilder.InsertData(
                table: "RoleMenuPermissions",
                columns: new[] { "MenuId", "RoleId", "PermissionLevel" },
                values: new object[] { 15, 1, "View" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleMenuPermissions",
                keyColumns: new[] { "MenuId", "RoleId" },
                keyValues: new object[] { 15, 1 });

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}
