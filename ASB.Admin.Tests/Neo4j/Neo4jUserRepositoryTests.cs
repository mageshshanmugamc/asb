namespace ASB.Admin.Tests.Neo4j;

using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Neo4j;
using global::Neo4j.Driver;
using Moq;
using Xunit;

public class Neo4jUserRepositoryTests
{
    private readonly Mock<INeo4jSessionFactory> _factoryMock;
    private readonly Mock<IAsyncSession> _sessionMock;
    private readonly Neo4jUserRepository _repository;

    public Neo4jUserRepositoryTests()
    {
        _factoryMock = new Mock<INeo4jSessionFactory>();
        _sessionMock = new Mock<IAsyncSession>();
        _factoryMock.Setup(f => f.OpenSession()).Returns(_sessionMock.Object);
        _repository = new Neo4jUserRepository(_factoryMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsUser()
    {
        var node = CreateUserNode(1, "user1", "u1@test.com", "hash");
        var record = CreateRecord("u", node);
        SetupRunAsync(CursorWithRecords(record));

        var result = await _repository.GetUserByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("user1", result!.Username);
        Assert.Equal("u1@test.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserNotFound_ReturnsNull()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetUserByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_UserExists_ReturnsUser()
    {
        var node = CreateUserNode(2, "user2", "u2@test.com", "hash");
        var record = CreateRecord("u", node);
        SetupRunAsync(CursorWithRecords(record));

        var result = await _repository.GetUserByEmailAsync("u2@test.com");

        Assert.NotNull(result);
        Assert.Equal("user2", result!.Username);
    }

    [Fact]
    public async Task GetUserByEmailAsync_NotFound_ReturnsNull()
    {
        SetupRunAsync(CursorWithRecords());

        var result = await _repository.GetUserByEmailAsync("none@test.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsUserWithId()
    {
        var node = CreateUserNode(10, "newuser", "new@test.com", "");
        var record = CreateRecord("u", node);
        SetupRunAsync(CursorWithRecords(record));

        var user = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "" };
        var result = await _repository.CreateUserAsync(user);

        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task AddUserToGroupAsync_AlreadyMember_ThrowsInvalidOperation()
    {
        var existingRecord = CreateRecord("r", Mock.Of<INode>());
        _sessionMock.Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(CursorWithRecords(existingRecord));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.AddUserToGroupAsync(1, 2));
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

        // FetchAsync / Current pattern (used by Neo4j SingleAsync, ToListAsync)
        int fetchIndex = -1;
        cursor.Setup(c => c.FetchAsync()).ReturnsAsync(() =>
        {
            fetchIndex++;
            return fetchIndex < records.Length;
        });
        if (records.Length > 0)
            cursor.Setup(c => c.Current).Returns(() => records[fetchIndex]);

        // IAsyncEnumerable pattern (used by LINQ SingleOrDefaultAsync, await foreach)
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

    private static INode CreateUserNode(int id, string username, string email, string passwordHash)
    {
        var node = new Mock<INode>();
        var props = new Dictionary<string, object>
        {
            ["id"] = id,
            ["username"] = username,
            ["email"] = email,
            ["passwordHash"] = passwordHash
        };
        node.Setup(n => n[It.IsAny<string>()]).Returns<string>(key => props[key]);
        node.Setup(n => n.Properties).Returns(props);
        return node.Object;
    }

    private static IRecord CreateRecord(string key, object value)
    {
        var record = new Mock<IRecord>();
        record.Setup(r => r[key]).Returns(value);
        return record.Object;
    }
}
