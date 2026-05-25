using Microsoft.AspNetCore.Mvc;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/hmac-test")]
public class HmacTestController : ControllerBase
{
    [HttpPost]
    public IActionResult Test([FromBody] object body)
    {
        return Ok(new
        {
            message = "HMAC request accepted",
            received = body
        });
    }
}