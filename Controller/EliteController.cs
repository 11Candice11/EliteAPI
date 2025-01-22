using EliteService.EliteServiceManager;
using EliteService.EliteServiceManager.Models.DTO;
using EliteService.EliteServiceManager.Models.Request;
using EliteService.EliteServiceManager.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace EliteService.Controller;

[Route($"api/elite/v1")]

public class EliteController(EliteServiceManager.EliteServiceManager eliteService, ILogger<EliteController> logger) : ControllerBase
{
    private readonly ILogger<EliteController> _logger = logger;
    private readonly IEliteServiceManager _eliteService = eliteService;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Invalid user input.");
        }

        bool isValid = await _eliteService.VerifyUserAsync(user.Username, user.Password);

        if (isValid)
        {
            return Ok(new { Message = "Login successful!" });
        }
        else
        {
            return Unauthorized(new { Message = "Invalid username or password." });
        }
    }

    
    [HttpPost("get-client-profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClientProfileResponse>> GetClientProfile([FromBody] ClientProfileRequest request)
    {
        var responseModel = await _eliteService.GetClientProfile(request);

        if (responseModel is null)
        {
            return NoContent();
        }

        return Ok(responseModel);
    }

}