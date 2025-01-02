using EliteAPI.EliteAPI;
using EliteAPI.Models.Request;
using EliteAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace EliteAPI.Controller;

[Route($"api/elite/v1")]

public class EliteController(EliteManager eliteService, ILogger<EliteController> logger) : ControllerBase
{
    private readonly ILogger<EliteController> _logger = logger;
    private readonly IEliteManager _eliteService = eliteService;
    
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