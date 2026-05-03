namespace ASB.Admin.Tests.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Neo4j;
using global::Neo4j.Driver;
using Moq;
using Xunit;

public class Neo4jRoleRepositoryTests
{
    private readonly Mock<INeo4jSessionFactory> _factoryMock;
    private readonly Mock<IAsyncSession> _sessionMock;
    private readonly Neo4jRoleRepository _repository;

    public Neo4jRoleRepositoryTests()
    {
        _factoryMock = new Mock<INeo4jSessionFactory>();
        _sessionMock = new Mock<IAsyncSession>();
        _factoryMock.Setup(f => f.OpenSession()).Returns(_sessionMock.Object);
        _repository = new Neo4jRoleRepository(_factoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRolesWithRelations()
    {
        var roleNode = CreateRoleNode(1, "Admin");
        var policies = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["policyId"] = 1, ["policyName"] = "FullAccess" }
        };
        var groups = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["groupId"] = 1, ["groupName"] = "Admins" }
        };

        var record = new Mock<IRecord>();
        record.Setup(r => r["r"]).Returns(roleNode);
        record.Setup(r => r["policies"]).Returns(policies);
        record.Setup(r => r["groups"]).Returns(groups);

        SetupRunAsync(CursorWithRecords(record.Object));

        var result = (await _repository.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Admin", result[0].Name);
        Assert.Single(result[0].RolePolicies);
        Assert.Single(result[0].UserGroupRoles);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsRoleWithRelations()
    {
        var roleNode = CreateRoleNode(1, "Editor");
        var policies = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["policyId"] = 2, ["policyName"] = "ReadOnly" }
        };
        var groups = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["groupId"] = 3, ["groupName"] = "Editors" }
        };

        var record = new Mock<IRecord>();
        record.Setup(r => r["r"]).Returns(roleNode);
        record.Setup(r => r["policies"]).Returns(policies);
        record.Setup(r => r["groups"]).Returns(groups);

        SetupRunAsync(CursorWithRecords(record.Object));

        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Editor", result!.Name);
        Assert.Single(result.RolePolicies);
        Assert.Equal("ReadOnly", result.RolePolicies.First().Policy.Name);
        Assert.Single(result.UserGroupRoles);
        Assert.Equal("Editors", result.UserGroupRoles.First().UserGroup.GroupName);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_NullPoliciesAndGroups_SkipsNulls()
    {
        var roleNode = CreateRoleNode(1, "Minimal");
        var policies = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["policyId"] = (object)null!, ["policyName"] = (object)null! }
        };
        var groups = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { ["groupId"] = (object)null!, ["groupName"] = (object)null! }
        };

        var record = new Mock<IRecord>();
        record.Setup(r => r["r"]).Returns(roleNode);
        record.Setup(r => r["policies"]).Returns(policies);
        record.Setup(r => r["groups"]).Returns(groups);

        SetupRunAsync(CursorWithRecords(record.Object));

        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result!.RolePolicies);
        Assert.Empty(result.UserGroupRoles);
    }

    [Fact]
    public async Task CreateAsync_ReturnsRoleWithId()
    {
        var roleNode = CreateRoleNode(5, "NewRole");
        var record = new Mock<IRecord>();
        record.Setup(r => r["r"]).Returns(roleNode);

        SetupRunAsync(CursorWithRecords(record.Object));

        var role = new Role { Name = "NewRole" };
        var result = await _repository.CreateAsync(role);

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task PolicyAssignmentExistsAsync_Exists_ReturnsTrue()
    {
        var record = new Mock<IRecord>();
        SetupRunAsync(CursorWithRecords(record.Object));

        var result = await _repository.PolicyAssignmentExistsAsync(1, 2);

        Assert.True(result);
    }

    [Fact]
    public async Task PolicyAssignmentExistsAsync_NotExists_ReturnsFalse()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.PolicyAssignmentExistsAsync(99, 99);

        Assert.False(result);
    }

    [Fact]
    public async Task AssignPolicyToRoleAsync_CallsSession()
    {
        SetupRunAsync(CursorWithRecords());

        await _repository.AssignPolicyToRoleAsync(1, 2);

        _sessionMock.Verify(s => s.RunAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
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

    private static INode CreateRoleNode(int id, string name)
    {
        var node = new Mock<INode>();
        var props = new Dictionary<string, object>
        {
            ["id"] = id,
            ["name"] = name
        };
        node.Setup(n => n[It.IsAny<string>()]).Returns<string>(key => props[key]);
        node.Setup(n => n.Properties).Returns(props);
        return node.Object;
    }
}
