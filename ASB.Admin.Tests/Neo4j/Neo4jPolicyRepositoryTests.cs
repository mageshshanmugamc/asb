namespace ASB.Admin.Tests.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Neo4j;
using global::Neo4j.Driver;
using Moq;
using Xunit;

public class Neo4jPolicyRepositoryTests
{
    private readonly Mock<INeo4jSessionFactory> _factoryMock;
    private readonly Mock<IAsyncSession> _sessionMock;
    private readonly Neo4jPolicyRepository _repository;

    public Neo4jPolicyRepositoryTests()
    {
        _factoryMock = new Mock<INeo4jSessionFactory>();
        _sessionMock = new Mock<IAsyncSession>();
        _factoryMock.Setup(f => f.OpenSession()).Returns(_sessionMock.Object);
        _repository = new Neo4jPolicyRepository(_factoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPolicies()
    {
        var node = CreatePolicyNode(1, "FullAccess", "Full access", "*", "*");
        var record = new Mock<IRecord>();
        record.Setup(r => r["p"]).Returns(node);

        SetupRunAsync(CursorWithRecords(record.Object));

        var result = (await _repository.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("FullAccess", result[0].Name);
        Assert.Equal("*", result[0].Resource);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsPolicy()
    {
        var node = CreatePolicyNode(2, "ReadOnly", "Read only", "*", "Read");
        var record = new Mock<IRecord>();
        record.Setup(r => r["p"]).Returns(node);

        SetupRunAsync(CursorWithRecords(record.Object));

        var result = await _repository.GetByIdAsync(2);

        Assert.NotNull(result);
        Assert.Equal("ReadOnly", result!.Name);
        Assert.Equal("Read", result.Action);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsPolicyWithId()
    {
        var node = CreatePolicyNode(5, "NewPolicy", "Desc", "Invoice", "Write");
        var record = new Mock<IRecord>();
        record.Setup(r => r["p"]).Returns(node);

        SetupRunAsync(CursorWithRecords(record.Object));

        var policy = new Policy { Name = "NewPolicy", Description = "Desc", Resource = "Invoice", Action = "Write" };
        var result = await _repository.CreateAsync(policy);

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_Found_ReturnsUpdatedPolicy()
    {
        var node = CreatePolicyNode(3, "Updated", "Updated desc", "User", "Delete");
        var record = new Mock<IRecord>();
        record.Setup(r => r["p"]).Returns(node);

        SetupRunAsync(CursorWithRecords(record.Object));

        var policy = new Policy { Id = 3, Name = "Updated", Description = "Updated desc", Resource = "User", Action = "Delete" };
        var result = await _repository.UpdateAsync(policy);

        Assert.Equal("Updated", result.Name);
        Assert.Equal("Delete", result.Action);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFound()
    {
        SetupRunAsync(CursorWithRecords());

        var policy = new Policy { Id = 999, Name = "X", Description = "X", Resource = "X", Action = "X" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(policy));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void SetupRunAsync(IResultCursor cursor)
    {
        _sessionMock.Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(cursor);
        _sessionMock.Setup(s => s.RunAsync(It.IsAny<string>()))
            .ReturnsAsync(cursor);
    }

    private static IResultCursor CursorWithRecords(params IRecord[] records)
    {
        var cursor = new Mock<IResultCursor>();

        int fetchIndex = -1;
        cursor.Setup(c => c.FetchAsync()).ReturnsAsync(() =>
        {
            fetchIndex++;
            return fetchIndex < records.Length;
        });
        if (records.Length > 0)
            cursor.Setup(c => c.Current).Returns(() => records[fetchIndex]);

        int enumIndex = -1;
        var asyncEnum = new Mock<IAsyncEnumerator<IRecord>>();
        asyncEnum.Setup(e => e.MoveNextAsync()).ReturnsAsync(() =>
        {
            enumIndex++;
            return enumIndex < records.Length;
        });
        if (records.Length > 0)
            asyncEnum.Setup(e => e.Current).Returns(() => records[enumIndex]);
        cursor.As<IAsyncEnumerable<IRecord>>()
            .Setup(c => c.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(asyncEnum.Object);

        return cursor.Object;
    }

    private static INode CreatePolicyNode(int id, string name, string description, string resource, string action)
    {
        var node = new Mock<INode>();
        var props = new Dictionary<string, object>
        {
            ["id"] = id,
            ["name"] = name,
            ["description"] = description,
            ["resource"] = resource,
            ["action"] = action
        };
        node.Setup(n => n[It.IsAny<string>()]).Returns<string>(key => props[key]);
        node.Setup(n => n.Properties).Returns(props);
        return node.Object;
    }
}
