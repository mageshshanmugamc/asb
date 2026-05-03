namespace ASB.Repositories.v1.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using global::Neo4j.Driver;

public class Neo4jUserGroupRepository : IUserGroupRepository
{
    private readonly INeo4jSessionFactory _factory;

    public Neo4jUserGroupRepository(INeo4jSessionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<UserGroup>> GetAllAsync()
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (g:UserGroup)
              OPTIONAL MATCH (u:User)-[m:MEMBER_OF]->(g)
              OPTIONAL MATCH (g)-[:HAS_ROLE]->(r:Role)
              RETURN g,
                     collect(DISTINCT {userId: u.id, username: u.username, email: u.email}) AS users,
                     collect(DISTINCT {roleId: r.id, roleName: r.name}) AS roles");

        var groups = new List<UserGroup>();
        await foreach (var record in result)
        {
            var group = MapGroup(record["g"].As<INode>());
            PopulateGroupRelations(group, record);
            groups.Add(group);
        }
        return groups;
    }

    public async Task<UserGroup?> GetByIdAsync(int id)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (g:UserGroup {id: $id})
              OPTIONAL MATCH (u:User)-[m:MEMBER_OF]->(g)
              OPTIONAL MATCH (g)-[:HAS_ROLE]->(r:Role)
              RETURN g,
                     collect(DISTINCT {userId: u.id, username: u.username, email: u.email}) AS users,
                     collect(DISTINCT {roleId: r.id, roleName: r.name}) AS roles",
            new { id });

        var record = await result.SingleOrDefaultAsync();
        if (record is null) return null;

        var group = MapGroup(record["g"].As<INode>());
        PopulateGroupRelations(group, record);
        return group;
    }

    public async Task<UserGroup> CreateAsync(UserGroup userGroup)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"CREATE (g:UserGroup {groupName: $groupName})
              SET g.id = id(g)
              RETURN g",
            new { groupName = userGroup.GroupName });

        var record = await result.SingleAsync();
        var node = record["g"].As<INode>();
        userGroup.Id = node["id"].As<int>();

        // Assign roles if provided
        foreach (var ugr in userGroup.UserGroupRoles)
        {
            await session.RunAsync(
                @"MATCH (g:UserGroup {id: $groupId}), (r:Role {id: $roleId})
                  CREATE (g)-[:HAS_ROLE]->(r)",
                new { groupId = userGroup.Id, roleId = ugr.RoleId });
        }

        return await GetByIdAsync(userGroup.Id) ?? userGroup;
    }

    public async Task<UserGroup> UpdateAsync(UserGroup userGroup)
    {
        await using var session = _factory.OpenSession();

        // Update group name
        var result = await session.RunAsync(
            @"MATCH (g:UserGroup {id: $id})
              SET g.groupName = $groupName
              RETURN g",
            new { id = userGroup.Id, groupName = userGroup.GroupName });

        if (await result.SingleOrDefaultAsync() is null)
            throw new KeyNotFoundException($"UserGroup with Id {userGroup.Id} not found.");

        // Remove existing role relationships
        await session.RunAsync(
            @"MATCH (g:UserGroup {id: $id})-[rel:HAS_ROLE]->(:Role)
              DELETE rel",
            new { id = userGroup.Id });

        // Create new role relationships
        foreach (var ugr in userGroup.UserGroupRoles)
        {
            await session.RunAsync(
                @"MATCH (g:UserGroup {id: $groupId}), (r:Role {id: $roleId})
                  CREATE (g)-[:HAS_ROLE]->(r)",
                new { groupId = userGroup.Id, roleId = ugr.RoleId });
        }

        return await GetByIdAsync(userGroup.Id) ?? userGroup;
    }

    public async Task AddUserToGroupAsync(int userId, int groupId)
    {
        await using var session = _factory.OpenSession();

        // Verify group exists
        var groupCheck = await session.RunAsync(
            "MATCH (g:UserGroup {id: $groupId}) RETURN g", new { groupId });
        if (await groupCheck.SingleOrDefaultAsync() is null)
            throw new KeyNotFoundException($"UserGroup with Id {groupId} not found.");

        // Verify user exists
        var userCheck = await session.RunAsync(
            "MATCH (u:User {id: $userId}) RETURN u", new { userId });
        if (await userCheck.SingleOrDefaultAsync() is null)
            throw new KeyNotFoundException($"User with Id {userId} not found.");

        await session.RunAsync(
            @"MATCH (u:User {id: $userId}), (g:UserGroup {id: $groupId})
              CREATE (u)-[:MEMBER_OF {assignedAt: datetime()}]->(g)",
            new { userId, groupId });
    }

    public async Task AssignRoleToGroupAsync(int groupId, int roleId)
    {
        await using var session = _factory.OpenSession();
        await session.RunAsync(
            @"MATCH (g:UserGroup {id: $groupId}), (r:Role {id: $roleId})
              CREATE (g)-[:HAS_ROLE]->(r)",
            new { groupId, roleId });
    }

    public async Task<bool> RoleAssignmentExistsAsync(int groupId, int roleId)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (g:UserGroup {id: $groupId})-[:HAS_ROLE]->(r:Role {id: $roleId})
              RETURN g",
            new { groupId, roleId });

        return await result.SingleOrDefaultAsync() is not null;
    }

    private static UserGroup MapGroup(INode node) => new()
    {
        Id = node["id"].As<int>(),
        GroupName = node["groupName"].As<string>()
    };

    private static void PopulateGroupRelations(UserGroup group, IRecord record)
    {
        var users = record["users"].As<List<IDictionary<string, object>>>();
        foreach (var u in users)
        {
            if (u["userId"] is null) continue;
            group.UserGroupMappings.Add(new UserGroupMapping
            {
                UserId = u["userId"].As<int>(),
                UserGroupId = group.Id,
                User = new User
                {
                    Id = u["userId"].As<int>(),
                    Username = u["username"].As<string>(),
                    Email = u["email"].As<string>()
                }
            });
        }

        var roles = record["roles"].As<List<IDictionary<string, object>>>();
        foreach (var r in roles)
        {
            if (r["roleId"] is null) continue;
            group.UserGroupRoles.Add(new UserGroupRole
            {
                UserGroupId = group.Id,
                RoleId = r["roleId"].As<int>(),
                Role = new Role
                {
                    Id = r["roleId"].As<int>(),
                    Name = r["roleName"].As<string>()
                }
            });
        }
    }
}
