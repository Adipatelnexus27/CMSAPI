using CMS.API.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AuthorizationDemoController : ControllerBase
{
    [HttpGet("admin-dashboard")]
    [Authorize(Roles = "Admin,Claim Manager")]
    [RequirePermission("Reports.View")]
    public IActionResult AdminDashboard()
    {
        return Ok(new { message = "Role and permission checks passed." });
    }

    [HttpGet("fraud-review")]
    [Authorize(Roles = "Fraud Analyst,Admin")]
    [RequirePermission("Fraud.Review")]
    public IActionResult FraudReview()
    {
        return Ok(new { message = "Fraud review access granted." });
    }
}
