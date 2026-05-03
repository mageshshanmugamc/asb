namespace ASB.Repositories.v1.Neo4j;

using global::Neo4j.Driver;

/// <summary>
/// Wraps the Neo4j IDriver to provide async session access.
/// Registered as a singleton; IDriver manages its own connection pool.
/// </summary>
public interface INeo4jSessionFactory : IAsyncDisposable
{
    IAsyncSession OpenSession();
}

public class Neo4jSessionFactory : INeo4jSessionFactory
{
    private readonly IDriver _driver;

    public Neo4jSessionFactory(string uri, string username, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
    }

    public IAsyncSession OpenSession() => _driver.AsyncSession();

    public async ValueTask DisposeAsync()
    {
        await _driver.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
