namespace ASB.Admin.Tests.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Neo4j;
using global::Neo4j.Driver;
using Moq;
using Xunit;

public class Neo4jUserGroupRepositoryTests
{
    private readonly Mock<INeo4jSessionFactory> _factoryMock;
    private readonly Mock<IAsyncSession> _sessionMock;
    private readonly Neo4jUserGroupRepository _repository;

    public Neo4jUserGroupRepositoryTests()
    {
        _factoryMock = new Mock<INeo4jSessionFactory>();
        _sessionMock = new Mock<IAsyncSession>();
        _factoryMock.Setup(f => f.OpenSession()).Returns(_sessionMock.Object);
        _repository = new Neo4jUserGroupRepository(_factoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsGroupWithRelations()
    {
        var groupNode = CreateGroupNode(1, "Admin");
        var users = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["userId"] = 1, ["username"] = "u1", ["email"] = "u1@t.com" }
        };
        var roles = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["roleId"] = 1, ["roleName"] = "Admin" }
        };

        var record = new Mock<IRecord>();
        record.Setup(r => r["g"]).Returns(groupNode);
        record.Setup(r => r["users"]).Returns(users);
        record.Setup(r => r["roles"]).Returns(roles);

        SetupRunAsync(CursorWithRecords(record.Object));

        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Admin", result!.GroupName);
        Assert.Single(result.UserGroupMappings);
        Assert.Single(result.UserGroupRoles);
    }

    [Fact]
    public async Task CreateAsync_ReturnsGroupWithId()
    {
        var groupNode = CreateGroupNode(5, "NewGroup");
        var record = new Mock<IRecord>();
        record.Setup(r => r["g"]).Returns(groupNode);

        SetupRunAsync(CursorWithRecords(record.Object));

        var group = new UserGroup { GroupName = "NewGroup" };
        var result = await _repository.CreateAsync(group);

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task AddUserToGroupAsync_GroupNotFound_ThrowsKeyNotFound()
    {
        SetupRunAsync(CursorWithRecords());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _repository.AddUserToGroupAsync(1, 999));
    }

    [Fact]
    public async Task RoleAssignmentExistsAsync_Exists_ReturnsTrue()
    {
        var record = new Mock<IRecord>();
        SetupRunAsync(CursorWithRecords(record.Object));

        var result = await _repository.RoleAssignmentExistsAsync(1, 2);

        Assert.True(result);
    }

    [Fact]
    public async Task RoleAssignmentExistsAsync_NotExists_ReturnsFalse()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.RoleAssignmentExistsAsync(99, 99);

        Assert.False(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void SetupRunAsync(IResultCursor cursor)
    {
        _sessionMock.Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
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

    private static INode CreateGroupNode(int id, string groupName)
    {
        var node = new Mock<INode>();
        var props = new Dictionary<string, object>
        {
            ["id"] = id,
            ["groupName"] = groupName
        };
        node.Setup(n => n[It.IsAny<string>()]).Returns<string>(key => props[key]);
        node.Setup(n => n.Properties).Returns(props);
        return node.Object;
    }
}
