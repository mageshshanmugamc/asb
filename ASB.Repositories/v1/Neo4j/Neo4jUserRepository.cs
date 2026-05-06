namespace ASB.Repositories.v1.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using global::Neo4j.Driver;

public class Neo4jUserRepository : IUserRepository
{
    private readonly INeo4jSessionFactory _factory;

    public Neo4jUserRepository(INeo4jSessionFactory factory)
    {
        _factory = factory;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            "MATCH (u:User {id: $id}) RETURN u",
            new { id });

        var record = await result.SingleOrDefaultAsync();
        return record is null ? null : MapUser(record["u"].As<INode>());
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            "MATCH (u:User {email: $email}) RETURN u",
            new { email });

        var record = await result.SingleOrDefaultAsync();
        return record is null ? null : MapUser(record["u"].As<INode>());
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (u:User)
              OPTIONAL MATCH (u)-[m:MEMBER_OF]->(g:UserGroup)
              RETURN u, collect({userGroupId: g.id, assignedAt: m.assignedAt, assignedBy: m.assignedBy}) AS memberships");

        var users = new List<User>();
        await foreach (var record in result)
        {
            var user = MapUser(record["u"].As<INode>());
            var memberships = record["memberships"].As<List<IDictionary<string, object>>>();
            foreach (var membership in memberships)
            {
                if (membership["userGroupId"] is null) continue;
                user.UserGroupMappings.Add(new UserGroupMapping
                {
                    UserId = user.Id,
                    UserGroupId = membership["userGroupId"].As<int>(),
                    AssignedAt = ConvertToDateTime(membership["assignedAt"]),
                    AssignedBy = membership["assignedBy"]?.As<string>()
                });
            }
            users.Add(user);
        }
        return users;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"CREATE (u:User {username: $username, email: $email, passwordHash: $passwordHash})
              SET u.id = id(u)
              RETURN u",
            new { username = user.Username, email = user.Email, passwordHash = user.PasswordHash });

        var record = await result.SingleAsync();
        var node = record["u"].As<INode>();
        user.Id = node["id"].As<int>();
        return user;
    }

    public async Task AddUserToGroupAsync(int userId, int userGroupId, string? assignedBy = null)
    {
        await using var session = _factory.OpenSession();

        // Check for existing membership
        var checkResult = await session.RunAsync(
            @"MATCH (u:User {id: $userId})-[r:MEMBER_OF]->(g:UserGroup {id: $groupId})
              RETURN r",
            new { userId, groupId = userGroupId });

        if (await checkResult.SingleOrDefaultAsync() is not null)
            throw new InvalidOperationException($"User {userId} is already a member of group {userGroupId}.");

        await session.RunAsync(
            @"MATCH (u:User {id: $userId}), (g:UserGroup {id: $groupId})
              CREATE (u)-[:MEMBER_OF {assignedAt: datetime(), assignedBy: $assignedBy}]->(g)",
            new { userId, groupId = userGroupId, assignedBy });
    }

    private static User MapUser(INode node) => new()
    {
        Id = node["id"].As<int>(),
        Username = node["username"].As<string>(),
        Email = node["email"].As<string>(),
        PasswordHash = node["passwordHash"].As<string>()
    };

    private static DateTime ConvertToDateTime(object? value)
    {
        if (value is null) return DateTime.UtcNow;
        if (value is ZonedDateTime zdt) return zdt.ToDateTimeOffset().UtcDateTime;
        if (value is LocalDateTime ldt) return ldt.ToDateTime();
        return DateTime.UtcNow;
    }

    public async Task<IEnumerable<User>> GetUserById(int userId)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (u:User {id: $userId})
              OPTIONAL MATCH (u)-[m:MEMBER_OF]->(g:UserGroup)
              RETURN u, collect({userGroupId: g.id, assignedAt: m.assignedAt, assignedBy: m.assignedBy}) AS memberships",
            new { userId });

        var users = new List<User>();
        await foreach (var record in result)
        {
            var user = MapUser(record["u"].As<INode>());
            var memberships = record["memberships"].As<List<IDictionary<string, object>>>();
            foreach (var membership in memberships)
            {
                if (membership["userGroupId"] is null) continue;
                user.UserGroupMappings.Add(new UserGroupMapping
                {
                    UserId = user.Id,
                    UserGroupId = membership["userGroupId"].As<int>(),
                    AssignedAt = ConvertToDateTime(membership["assignedAt"]),
                    AssignedBy = membership["assignedBy"]?.As<string>()
                });
            }
            users.Add(user);
        }
        return users;
    }

}
