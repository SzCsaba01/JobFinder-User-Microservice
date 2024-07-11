using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.Data.Access.Helpers;
using User.Data.Access.Helpers.DTO.Authentication;
using User.Services.Contracts;

namespace User.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequestDto request)
    {
        var response = await _authenticationService.LoginAsync(request);

        HttpContext.Response.Cookies.Append("token", response.Token,
            new CookieOptions
            {
                Expires = DateTime.Now.AddHours(AppConstants.JWT_TOKEN_VALIDATION_TIME),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None
            });

        return Ok(response.Role);
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost("Logout")]
    public Task<IActionResult> Logout()
    {
        HttpContext.Response.Cookies.Delete("token");

        return Task.FromResult<IActionResult>(Ok());
    }
}
