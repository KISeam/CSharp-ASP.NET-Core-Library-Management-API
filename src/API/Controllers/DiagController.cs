using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace LibraryAPI.API.Controllers;

[Route("api/diag")]
[ApiController]
public class DiagController : ControllerBase
{
    [HttpGet("hash/{password}")]
    public IActionResult GetHash(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        return Ok(new { password, hash, isValid });
    }
}
