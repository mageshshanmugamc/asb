namespace ASB.Admin.Tests.Neo4j;

using ASB.Repositories.v1.Neo4j;
using global::Neo4j.Driver;
using Moq;
using Xunit;

public class Neo4jMenuRepositoryTests
{
    private readonly Mock<INeo4jSessionFactory> _factoryMock;
    private readonly Mock<IAsyncSession> _sessionMock;
    private readonly Neo4jMenuRepository _repository;

    public Neo4jMenuRepositoryTests()
    {
        _factoryMock = new Mock<INeo4jSessionFactory>();
        _sessionMock = new Mock<IAsyncSession>();
        _factoryMock.Setup(f => f.OpenSession()).Returns(_sessionMock.Object);
        _repository = new Neo4jMenuRepository(_factoryMock.Object);
    }

    [Fact]
    public async Task GetUserRoleIdsAsync_ReturnsRoleIds()
    {
        var rec1 = CreateRecord("roleId", 1);
        var rec2 = CreateRecord("roleId", 3);
        SetupRunAsync(CursorWithRecords(rec1, rec2));

        var result = await _repository.GetUserRoleIdsAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public async Task GetUserRoleIdsAsync_NoRoles_ReturnsEmpty()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetUserRoleIdsAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPolicyNamesByRoleIdsAsync_ReturnsPolicyNames()
    {
        var rec1 = CreateRecord("policyName", "FullAccess");
        var rec2 = CreateRecord("policyName", "ReadOnly");
        SetupRunAsync(CursorWithRecords(rec1, rec2));

        var result = await _repository.GetPolicyNamesByRoleIdsAsync(new[] { 1 });

        Assert.Equal(2, result.Count);
        Assert.Contains("FullAccess", result);
        Assert.Contains("ReadOnly", result);
    }

    [Fact]
    public async Task GetPolicyNamesByRoleIdsAsync_NoMatches_ReturnsEmpty()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetPolicyNamesByRoleIdsAsync(new[] { 999 });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMenusByRoleIdsAsync_NoMenus_ReturnsEmpty()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetMenusByRoleIdsAsync(new[] { 999 });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMenusByRoleIdsAsync_ReturnsMenusWithPermissions()
    {
        var menuNode = new Mock<INode>();
        var props = new Dictionary<string, object>
        {
            ["id"] = 1,
            ["name"] = "Dashboard",
            ["route"] = "/dashboard",
            ["icon"] = "dashboard",
            ["displayOrder"] = 1
        };
        menuNode.Setup(n => n[It.IsAny<string>()]).Returns<string>(key => props.GetValueOrDefault(key)!);
        menuNode.Setup(n => n.Properties).Returns(props);

        var permissions = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["roleId"] = 1, ["roleName"] = "Admin", ["permissionLevel"] = "FullControl" }
        };

        var record = new Mock<IRecord>();
        record.Setup(r => r["m"]).Returns(menuNode.Object);
        record.Setup(r => r["permissions"]).Returns(permissions);

        SetupRunAsync(CursorWithRecords(record.Object));

        var result = await _repository.GetMenusByRoleIdsAsync(new[] { 1 });

        Assert.Single(result);
        Assert.Equal("Dashboard", result[0].Name);
        Assert.Single(result[0].RoleMenuPermissions);
        Assert.Equal("Admin", result[0].RoleMenuPermissions.First().Role.Name);
    }

    // ── Helpers ────────────────────────────────────────────────────────

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

    private static IRecord CreateRecord(string key, object value)
    {
        var record = new Mock<IRecord>();
        record.Setup(r => r[key]).Returns(value);
        return record.Object;
    }
}
