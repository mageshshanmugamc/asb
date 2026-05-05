namespace ASB.Admin.v1.Controllers
{
    using System.IdentityModel.Tokens.Jwt;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    /// <summary>
    /// Controller responsible for token exchange. It accepts a Keycloak token (already validated by middleware)
    /// and returns an app-specific token containing roles and allowed menus.
    /// </summary>
    public class AuthController : ControllerBase
    {
        private readonly IAuthTokenService _authTokenService;

        public AuthController(IAuthTokenService authTokenService)
        {
            _authTokenService = authTokenService;
        }

        /// <summary>
        /// Token exchange endpoint. Accepts the Keycloak token (already validated by middleware)
        /// and returns an app-specific token containing roles and allowed menus.
        /// Accepts application/x-www-form-urlencoded with grant_type=urn:ietf:params:oauth:grant-type:token-exchange
        /// </summary>
        [HttpPost("token")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Token(
            [FromForm] string grant_type,
            [FromForm] string subject_token)
        {
            if (grant_type != "urn:ietf:params:oauth:grant-type:token-exchange")
                return BadRequest(new { error = "unsupported_grant_type" });

            if (string.IsNullOrWhiteSpace(subject_token))
                return BadRequest(new { error = "invalid_request", error_description = "subject_token is required." });

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(subject_token))
                return BadRequest(new { error = "invalid_token", error_description = "subject_token is not a valid JWT." });

            var jwt = handler.ReadJwtToken(subject_token);

            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                        ?? jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "invalid_token", error_description = "Token has no email claim." });

            var dto = new GenerateTokenDto
            {
                Username = username ?? email,
                Email = email
            };

            var result = await _authTokenService.GenerateAppTokenAsync(dto);

            return Ok(new
            {
                access_token = result.Token,
                token_type = "Bearer",
                expires_at = result.ExpiresAt
            });
        }
    }
}
