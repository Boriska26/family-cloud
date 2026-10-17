using FamilyCloud.Application.Dtos;
using FamilyCloud.Application.Exceptions;
using FamilyCloud.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCloud.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginService _loginService;

    public AuthController(LoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login(
        [FromBody] LoginRequestDto loginRequestDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _loginService.Login(loginRequestDto.Login, loginRequestDto.Password, cancellationToken);

            return Ok(result);
        }
        catch (InvalidCredentialsException exp)
        {
            return Unauthorized(exp.Message);
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated;
        var isAdmin = User.IsInRole(Role.Administrator.ToString());
        
        return Ok(new { IsAuthenticated = isAuthenticated, IsAdmin = isAdmin });
    }
}