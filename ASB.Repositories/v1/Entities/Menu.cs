namespace ASB.Repositories.v1.Entities;

/// <summary>
/// Represents a menu/page item in the application's navigation hierarchy.
/// Menus can be nested (parent-child) to form a tree structure.
/// </summary>
public class Menu
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public int? ParentMenuId { get; set; }

    public Menu? ParentMenu { get; set; }
    public ICollection<Menu> Children { get; set; } = [];
    public ICollection<RoleMenuPermission> RoleMenuPermissions { get; set; } = [];
}
