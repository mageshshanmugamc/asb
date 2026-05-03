namespace ASB.Admin.Tests.Controllers;

using ASB.Admin.v1.Controllers;
using ASB.Services.v1.Dtos;
using ASB.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class AuthControllerTests
{
    private readonly Mock<IAuthTokenService> _authTokenServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authTokenServiceMock = new Mock<IAuthTokenService>();
        _controller = new AuthController(_authTokenServiceMock.Object);
    }

    [Fact]
    public async Task Token_UnsupportedGrantType_ReturnsBadRequest()
    {
        var result = await _controller.Token("invalid_grant_type", "some_token");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Token_EmptySubjectToken_ReturnsBadRequest()
    {
        var grantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        var result = await _controller.Token(grantType, "");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Token_NullSubjectToken_ReturnsBadRequest()
    {
        var grantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        var result = await _controller.Token(grantType, null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Token_InvalidJwt_ReturnsBadRequest()
    {
        var grantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        var result = await _controller.Token(grantType, "not.a.valid.jwt");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Token_ValidToken_ReturnsOk()
    {
        var grantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        // Create a minimal valid JWT with email claim
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}")).TrimEnd('=');
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"email\":\"test@example.com\",\"preferred_username\":\"testuser\"}")).TrimEnd('=');
        var subjectToken = $"{header}.{payload}.";

        var expectedResult = new AppTokenDto
        {
            Token = "app_token_123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Roles = ["Admin"],
            Menus = []
        };

        _authTokenServiceMock
            .Setup(s => s.GenerateAppTokenAsync(It.IsAny<GenerateTokenDto>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.Token(grantType, subjectToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Token_ValidTokenWithoutEmail_ReturnsBadRequest()
    {
        var grantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        // JWT without email claim
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}")).TrimEnd('=');
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"sub\":\"12345\",\"name\":\"Test User\"}")).TrimEnd('=');
        var subjectToken = $"{header}.{payload}.";

        var result = await _controller.Token(grantType, subjectToken);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Token_ValidRequest_CallsGenerateAppTokenAsync()
    {
        var grantType = "urn:ietf:params:oauth:grant-type:token-exchange";
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}")).TrimEnd('=');
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"email\":\"user@test.com\",\"preferred_username\":\"user1\"}")).TrimEnd('=');
        var subjectToken = $"{header}.{payload}.";

        _authTokenServiceMock
            .Setup(s => s.GenerateAppTokenAsync(It.Is<GenerateTokenDto>(d =>
                d.Email == "user@test.com" && d.Username == "user1")))
            .ReturnsAsync(new AppTokenDto { Token = "token", ExpiresAt = DateTime.UtcNow.AddHours(1) });

        await _controller.Token(grantType, subjectToken);

        _authTokenServiceMock.Verify(
            s => s.GenerateAppTokenAsync(It.Is<GenerateTokenDto>(d =>
                d.Email == "user@test.com" && d.Username == "user1")),
            Times.Once);
    }
}
