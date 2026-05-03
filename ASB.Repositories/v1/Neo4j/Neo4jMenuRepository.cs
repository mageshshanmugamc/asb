namespace ASB.Repositories.v1.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using global::Neo4j.Driver;

public class Neo4jMenuRepository : IMenuRepository
{
    private readonly INeo4jSessionFactory _factory;

    public Neo4jMenuRepository(INeo4jSessionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Menu>> GetMenusByRoleIdsAsync(IEnumerable<int> roleIds)
    {
        var roleIdList = roleIds.ToList();
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (r:Role)-[p:HAS_MENU_PERMISSION]->(m:Menu)
              WHERE r.id IN $roleIds
              RETURN DISTINCT m, collect(DISTINCT {roleId: r.id, roleName: r.name, permissionLevel: p.permissionLevel}) AS permissions
              ORDER BY m.displayOrder",
            new { roleIds = roleIdList });

        var menus = new List<Menu>();
        await foreach (var record in result)
        {
            var node = record["m"].As<INode>();
            var menu = MapMenu(node);

            var permissions = record["permissions"].As<List<IDictionary<string, object>>>();
            foreach (var perm in permissions)
            {
                menu.RoleMenuPermissions.Add(new RoleMenuPermission
                {
                    RoleId = perm["roleId"].As<int>(),
                    MenuId = menu.Id,
                    PermissionLevel = perm["permissionLevel"]?.As<string>() ?? "View",
                    Role = new Role
                    {
                        Id = perm["roleId"].As<int>(),
                        Name = perm["roleName"].As<string>()
                    }
                });
            }

            menus.Add(menu);
        }
        return menus;
    }

    public async Task<List<int>> GetUserRoleIdsAsync(int userId)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (u:User {id: $userId})-[:MEMBER_OF]->(g:UserGroup)-[:HAS_ROLE]->(r:Role)
              RETURN DISTINCT r.id AS roleId",
            new { userId });

        var roleIds = new List<int>();
        await foreach (var record in result)
        {
            roleIds.Add(record["roleId"].As<int>());
        }
        return roleIds;
    }

    public async Task<List<string>> GetPolicyNamesByRoleIdsAsync(IEnumerable<int> roleIds)
    {
        var roleIdList = roleIds.ToList();
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (r:Role)-[:HAS_POLICY]->(p:Policy)
              WHERE r.id IN $roleIds
              RETURN DISTINCT p.name AS policyName",
            new { roleIds = roleIdList });

        var policyNames = new List<string>();
        await foreach (var record in result)
        {
            policyNames.Add(record["policyName"].As<string>());
        }
        return policyNames;
    }

    private static Menu MapMenu(INode node) => new()
    {
        Id = node["id"].As<int>(),
        Name = node["name"].As<string>(),
        Route = node["route"].As<string>(),
        Icon = node.Properties.ContainsKey("icon") ? node["icon"]?.As<string>() : null,
        DisplayOrder = node["displayOrder"].As<int>(),
        ParentMenuId = node.Properties.ContainsKey("parentMenuId") ? node["parentMenuId"]?.As<int?>() : null
    };
}
