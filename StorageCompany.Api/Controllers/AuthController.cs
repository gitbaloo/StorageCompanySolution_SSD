using Microsoft.AspNetCore.Mvc;
using StorageCompany.Core.Entities;
using StorageCompany.Core.Interfaces.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISecurityService securityService) : ControllerBase
{

    [EnableRateLimiting("AuthPolicy")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthLoginRequest request)
    {
        var response = await securityService.Login(request);
        return Ok(response);
    }

    [EnableRateLimiting("AuthPolicy")]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] AuthRegisterRequest request)
    {
        var response = await securityService.Register(request);
        return Ok(response);
    }
    
}