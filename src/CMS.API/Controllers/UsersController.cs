using CMS.API.Middlewares;
using CMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [RequirePermission("Users.Manage")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _authService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }
}

