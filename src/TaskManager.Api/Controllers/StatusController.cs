using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("public")]
    public IActionResult Public() => Ok(new
    {
        status  = "ok",
        service = "TaskManager.Api",
        access  = "public",
        timestamp = DateTime.UtcNow
    });

    [Authorize]
    [HttpGet("private")]
    public IActionResult Private()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        var email  = User.FindFirstValue(ClaimTypes.Email)
                  ?? User.FindFirstValue("email");
        var name   = User.FindFirstValue(ClaimTypes.Name)
                  ?? User.FindFirstValue("name");

        return Ok(new
        {
            status    = "ok",
            service   = "TaskManager.Api",
            access    = "private",
            timestamp = DateTime.UtcNow,
            user      = new { id = userId, email, name }
        });
    }
}
