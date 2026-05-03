namespace ASB.Repositories.v1.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Interfaces;
using global::Neo4j.Driver;

public class Neo4jPolicyRepository : IPolicyRepository
{
    private readonly INeo4jSessionFactory _factory;

    public Neo4jPolicyRepository(INeo4jSessionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Policy>> GetAllAsync()
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync("MATCH (p:Policy) RETURN p");

        var policies = new List<Policy>();
        await foreach (var record in result)
        {
            policies.Add(MapPolicy(record["p"].As<INode>()));
        }
        return policies;
    }

    public async Task<Policy?> GetByIdAsync(int id)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            "MATCH (p:Policy {id: $id}) RETURN p",
            new { id });

        var record = await result.SingleOrDefaultAsync();
        return record is null ? null : MapPolicy(record["p"].As<INode>());
    }

    public async Task<Policy> CreateAsync(Policy policy)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"CREATE (p:Policy {name: $name, description: $description, resource: $resource, action: $action})
              SET p.id = id(p)
              RETURN p",
            new { name = policy.Name, description = policy.Description, resource = policy.Resource, action = policy.Action });

        var record = await result.SingleAsync();
        var node = record["p"].As<INode>();
        policy.Id = node["id"].As<int>();
        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy)
    {
        await using var session = _factory.OpenSession();
        var result = await session.RunAsync(
            @"MATCH (p:Policy {id: $id})
              SET p.name = $name, p.description = $description, p.resource = $resource, p.action = $action
              RETURN p",
            new { id = policy.Id, name = policy.Name, description = policy.Description, resource = policy.Resource, action = policy.Action });

        var record = await result.SingleOrDefaultAsync();
        if (record is null)
            throw new KeyNotFoundException($"Policy with Id {policy.Id} not found.");

        return MapPolicy(record["p"].As<INode>());
    }

    private static Policy MapPolicy(INode node) => new()
    {
        Id = node["id"].As<int>(),
        Name = node["name"].As<string>(),
        Description = node["description"].As<string>(),
        Resource = node["resource"].As<string>(),
        Action = node["action"].As<string>()
    };
}
