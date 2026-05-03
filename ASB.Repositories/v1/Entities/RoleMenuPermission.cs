namespace ASB.Repositories.v1.Entities;

/// <summary>
/// Maps which roles have access to which menu items.
/// </summary>
public class RoleMenuPermission
{
    public int RoleId { get; set; }
    public int MenuId { get; set; }

    /// <summary>
    /// Permission level: View, Edit, FullControl.
    /// </summary>
    public string PermissionLevel { get; set; } = "View";

    public Role Role { get; set; } = null!;
    public Menu Menu { get; set; } = null!;
}
