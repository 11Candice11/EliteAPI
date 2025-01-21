using EliteService.EliteServiceManager;
using EliteService.EliteServiceManager.Models.Request;
using EliteService.EliteServiceManager.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace EliteService.Controller;

[Route($"api/elite/v1")]

public class EliteController(EliteServiceManager.EliteServiceManager eliteService, ILogger<EliteController> logger) : ControllerBase
{
    private readonly ILogger<EliteController> _logger = logger;
    private readonly IEliteServiceManager _eliteService = eliteService;
    
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