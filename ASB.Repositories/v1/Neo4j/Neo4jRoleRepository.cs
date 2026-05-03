namespace ASB.Repositories.v1.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using global::Neo4j.Driver;

public class Neo4jRoleRepository : IRoleRepository
{
    private readonly INeo4jSessionFactory _factory;

    public Neo4jRoleRepository(INeo4jSessionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (r:Role)
              OPTIONAL MATCH (r)-[:HAS_POLICY]->(p:Policy)
              OPTIONAL MATCH (g:UserGroup)-[:HAS_ROLE]->(r)
              RETURN r,
                     collect(DISTINCT {policyId: p.id, policyName: p.name}) AS policies,
                     collect(DISTINCT {groupId: g.id, groupName: g.groupName}) AS groups");

        var roles = new List<Role>();
        await foreach (var record in result)
        {
            var role = MapRole(record["r"].As<INode>());
            PopulateRoleRelations(role, record);
            roles.Add(role);
        }
        return roles;
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (r:Role {id: $id})
              OPTIONAL MATCH (r)-[:HAS_POLICY]->(p:Policy)
              OPTIONAL MATCH (g:UserGroup)-[:HAS_ROLE]->(r)
              RETURN r,
                     collect(DISTINCT {policyId: p.id, policyName: p.name}) AS policies,
                     collect(DISTINCT {groupId: g.id, groupName: g.groupName}) AS groups",
            new { id });

        var record = await result.SingleOrDefaultAsync();
        if (record is null) return null;

        var role = MapRole(record["r"].As<INode>());
        PopulateRoleRelations(role, record);
        return role;
    }

    public async Task<Role> CreateAsync(Role role)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"CREATE (r:Role {name: $name})
              SET r.id = id(r)
              RETURN r",
            new { name = role.Name });

        var record = await result.SingleAsync();
        var node = record["r"].As<INode>();
        role.Id = node["id"].As<int>();
        return role;
    }

    public async Task AssignPolicyToRoleAsync(int roleId, int policyId)
    {
        await using var session = _factory.OpenSession();
        await session.RunAsync(
            @"MATCH (r:Role {id: $roleId}), (p:Policy {id: $policyId})
              CREATE (r)-[:HAS_POLICY]->(p)",
            new { roleId, policyId });
    }

    public async Task<bool> PolicyAssignmentExistsAsync(int roleId, int policyId)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (r:Role {id: $roleId})-[:HAS_POLICY]->(p:Policy {id: $policyId})
              RETURN r",
            new { roleId, policyId });

        return await result.SingleOrDefaultAsync() is not null;
    }

    private static Role MapRole(INode node) => new()
    {
        Id = node["id"].As<int>(),
        Name = node["name"].As<string>()
    };

    private static void PopulateRoleRelations(Role role, IRecord record)
    {
        var policies = record["policies"].As<List<IDictionary<string, object>>>();
        foreach (var p in policies)
        {
            if (p["policyId"] is null) continue;
            role.RolePolicies.Add(new RolePolicy
            {
                RoleId = role.Id,
                PolicyId = p["policyId"].As<int>(),
                Policy = new Policy
                {
                    Id = p["policyId"].As<int>(),
                    Name = p["policyName"].As<string>(),
                    Description = string.Empty,
                    Resource = string.Empty,
                    Action = string.Empty
                }
            });
        }

        var groups = record["groups"].As<List<IDictionary<string, object>>>();
        foreach (var g in groups)
        {
            if (g["groupId"] is null) continue;
            role.UserGroupRoles.Add(new UserGroupRole
            {
                RoleId = role.Id,
                UserGroupId = g["groupId"].As<int>(),
                UserGroup = new UserGroup
                {
                    Id = g["groupId"].As<int>(),
                    GroupName = g["groupName"].As<string>()
                }
            });
        }
    }
}
