using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BotAdminUI.Services;

namespace BotAdminUI.Controllers;

[Route("account")]
public class AccountController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Discord");
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> LogoutAsync(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect(returnUrl);
    }
}

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IDiscordUserService _discordUserService;

    public AuthController(IConfiguration config, IDiscordUserService discordUserService)
    {
        _config = config;
        _discordUserService = discordUserService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" });
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("guilds")]
    [Authorize(AuthenticationSchemes = "Discord")]
    public async Task<IActionResult> GetGuilds()
    {
        var guilds = await _discordUserService.GetGuildsAsync();
        return Ok(guilds);
    }

    [HttpGet("token")]
    [Authorize(AuthenticationSchemes = "Discord")]
    public object GetToken()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        string key = _config["Jwt:EncryptionKey"]!;
        string issuer = _config["Jwt:Issuer"]!;
        string audience = _config["Jwt:Audience"]!;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var permClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("discordId", userId)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            permClaims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials
            );

        var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);

        return new
        {
            ApiToken = jwt_token
        };
    }
}
