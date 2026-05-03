namespace ASB.Admin.Tests.Authorization;

using System.Security.Claims;
using ASB.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;

public class PermissionHandlerTests
{
    private readonly PermissionHandler _handler = new();

    private static AuthorizationHandlerContext CreateContext(PermissionRequirement requirement, IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        return new AuthorizationHandlerContext([requirement], principal, null);
    }

    [Fact]
    public async Task HandleAsync_UserHasExactPermission_Succeeds()
    {
        var requirement = new PermissionRequirement(Policies.ReadOnly);
        var claims = new[] { new Claim("permissions", "ReadOnly") };

        var context = CreateContext(requirement, claims);
        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserHasFullAccess_SucceedsForAnyPolicy()
    {
        var requirement = new PermissionRequirement(Policies.ManageUsers);
        var claims = new[] { new Claim("permissions", "FullAccess") };

        var context = CreateContext(requirement, claims);
        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserLacksPermission_DoesNotSucceed()
    {
        var requirement = new PermissionRequirement(Policies.ManageUsers);
        var claims = new[] { new Claim("permissions", "ReadOnly") };

        var context = CreateContext(requirement, claims);
        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserHasNoPermissions_DoesNotSucceed()
    {
        var requirement = new PermissionRequirement(Policies.ReadOnly);
        var claims = Array.Empty<Claim>();

        var context = CreateContext(requirement, claims);
        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserHasMultiplePermissions_SucceedsIfOneMatches()
    {
        var requirement = new PermissionRequirement(Policies.AuditAccess);
        var claims = new[]
        {
            new Claim("permissions", "ReadOnly"),
            new Claim("permissions", "AuditAccess")
        };

        var context = CreateContext(requirement, claims);
        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_FullAccessOverridesAllPolicies()
    {
        var policies = new[] { Policies.ReadOnly, Policies.ManageUsers, Policies.AuditAccess, Policies.FullAccess };
        var claims = new[] { new Claim("permissions", "FullAccess") };

        foreach (var policy in policies)
        {
            var requirement = new PermissionRequirement(policy);
            var context = CreateContext(requirement, claims);
            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded, $"FullAccess should satisfy {policy}");
        }
    }
}
