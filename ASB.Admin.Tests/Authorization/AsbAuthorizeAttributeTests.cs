namespace ASB.Admin.Tests.Authorization;

using System.Security.Claims;
using ASB.Authorization;
using ASB.ErrorHandler.v1.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

public class AsbAuthorizeAttributeTests
{
    private static AuthorizationFilterContext CreateFilterContext(
        ClaimsPrincipal? user = null,
        bool allowAnonymous = false)
    {
        var httpContext = new DefaultHttpContext();
        if (user is not null)
        {
            httpContext.User = user;
        }

        var actionDescriptor = new ActionDescriptor();
        if (allowAnonymous)
        {
            actionDescriptor.EndpointMetadata =
            [
                new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute()
            ];
        }
        else
        {
            actionDescriptor.EndpointMetadata = [];
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        return new AuthorizationFilterContext(actionContext, []);
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(params string[] permissions)
    {
        var claims = permissions.Select(p => new Claim("permissions", p)).ToList();
        claims.Add(new Claim(ClaimTypes.Name, "testuser"));
        var identity = new ClaimsIdentity(claims, "TestScheme");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        var identity = new ClaimsIdentity(); // no auth type = unauthenticated
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void OnAuthorization_AllowAnonymous_SkipsAllChecks()
    {
        var attribute = new AsbAuthorizeAttribute(Policies.FullAccess);
        var context = CreateFilterContext(user: null, allowAnonymous: true);

        var act = () => attribute.OnAuthorization(context);

        act.Should().NotThrow();
    }

    [Fact]
    public void OnAuthorization_UnauthenticatedUser_ThrowsUnAuthorizedException()
    {
        var attribute = new AsbAuthorizeAttribute();
        var context = CreateFilterContext(user: CreateUnauthenticatedUser());

        var act = () => attribute.OnAuthorization(context);

        act.Should().Throw<UnAuthorizedException>()
            .WithMessage("*Authentication is required*");
    }

    [Fact]
    public void OnAuthorization_NullIdentity_ThrowsUnAuthorizedException()
    {
        var attribute = new AsbAuthorizeAttribute();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor { EndpointMetadata = [] });
        var context = new AuthorizationFilterContext(actionContext, []);

        var act = () => attribute.OnAuthorization(context);

        act.Should().Throw<UnAuthorizedException>();
    }

    [Fact]
    public void OnAuthorization_AuthenticatedUser_NoPolicyRequired_Succeeds()
    {
        var attribute = new AsbAuthorizeAttribute();
        var context = CreateFilterContext(user: CreateAuthenticatedUser("ReadOnly"));

        var act = () => attribute.OnAuthorization(context);

        act.Should().NotThrow();
    }

    [Fact]
    public void OnAuthorization_UserHasExactPermission_Succeeds()
    {
        var attribute = new AsbAuthorizeAttribute(Policies.ReadOnly);
        var context = CreateFilterContext(user: CreateAuthenticatedUser("ReadOnly"));

        var act = () => attribute.OnAuthorization(context);

        act.Should().NotThrow();
    }

    [Fact]
    public void OnAuthorization_UserHasFullAccess_SucceedsForAnyPolicy()
    {
        var attribute = new AsbAuthorizeAttribute(Policies.ManageUsers);
        var context = CreateFilterContext(user: CreateAuthenticatedUser("FullAccess"));

        var act = () => attribute.OnAuthorization(context);

        act.Should().NotThrow();
    }

    [Fact]
    public void OnAuthorization_UserLacksPermission_ThrowsForbiddenAccessException()
    {
        var attribute = new AsbAuthorizeAttribute(Policies.FullAccess);
        var context = CreateFilterContext(user: CreateAuthenticatedUser("ReadOnly"));

        var act = () => attribute.OnAuthorization(context);

        act.Should().Throw<ForbiddenAccessException>()
            .WithMessage("*Required permission 'FullAccess' is not granted*");
    }

    [Fact]
    public void OnAuthorization_UserHasNoPermissions_ThrowsForbiddenAccessException()
    {
        var attribute = new AsbAuthorizeAttribute(Policies.ReadOnly);
        var context = CreateFilterContext(user: CreateAuthenticatedUser());

        var act = () => attribute.OnAuthorization(context);

        act.Should().Throw<ForbiddenAccessException>()
            .WithMessage("*No permissions found*");
    }

    [Fact]
    public void OnAuthorization_UserHasMultiplePermissions_SucceedsIfOneMatches()
    {
        var attribute = new AsbAuthorizeAttribute(Policies.AuditAccess);
        var context = CreateFilterContext(user: CreateAuthenticatedUser("ReadOnly", "AuditAccess"));

        var act = () => attribute.OnAuthorization(context);

        act.Should().NotThrow();
    }

    [Fact]
    public void OnAuthorization_FullAccessOverridesAllPolicies()
    {
        var policies = new[] { Policies.ReadOnly, Policies.ManageUsers, Policies.AuditAccess, Policies.FullAccess };

        foreach (var policy in policies)
        {
            var attribute = new AsbAuthorizeAttribute(policy);
            var context = CreateFilterContext(user: CreateAuthenticatedUser("FullAccess"));

            var act = () => attribute.OnAuthorization(context);

            act.Should().NotThrow($"FullAccess should grant access for policy '{policy}'");
        }
    }

    [Fact]
    public void Constructor_NoPolicy_PolicyIsNull()
    {
        var attribute = new AsbAuthorizeAttribute();

        attribute.Policy.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithPolicy_PolicyIsSet()
    {
        var attribute = new AsbAuthorizeAttribute(Policies.ManageUsers);

        attribute.Policy.Should().Be(Policies.ManageUsers);
    }
}
