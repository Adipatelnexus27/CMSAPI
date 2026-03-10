using CMS.Application.DTOs;
using CMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequestDto request, CancellationToken cancellationToken)
    {
        await _authService.RevokeAsync(request, cancellationToken);
        return NoContent();
    }
}

