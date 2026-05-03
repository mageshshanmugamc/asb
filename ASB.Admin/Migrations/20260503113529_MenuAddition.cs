using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ASB.Admin.Migrations
{
    /// <inheritdoc />
    public partial class MenuAddition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "DisplayOrder", "Icon", "Name", "ParentMenuId", "Route" },
                values: new object[,]
                {
                    { 13, 4, "policy", "Policies", 2, "/users/policies" },
                    { 14, 3, "menu", "Menus", 4, "/settings/menus" }
                });

            migrationBuilder.InsertData(
                table: "RoleMenuPermissions",
                columns: new[] { "MenuId", "RoleId", "PermissionLevel" },
                values: new object[,]
                {
                    { 13, 1, "View" },
                    { 14, 1, "View" },
                    { 13, 3, "View" },
                    { 14, 3, "View" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleMenuPermissions",
                keyColumns: new[] { "MenuId", "RoleId" },
                keyValues: new object[] { 13, 1 });

            migrationBuilder.DeleteData(
                table: "RoleMenuPermissions",
                keyColumns: new[] { "MenuId", "RoleId" },
                keyValues: new object[] { 14, 1 });

            migrationBuilder.DeleteData(
                table: "RoleMenuPermissions",
                keyColumns: new[] { "MenuId", "RoleId" },
                keyValues: new object[] { 13, 3 });

            migrationBuilder.DeleteData(
                table: "RoleMenuPermissions",
                keyColumns: new[] { "MenuId", "RoleId" },
                keyValues: new object[] { 14, 3 });

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 14);
        }
    }
}
