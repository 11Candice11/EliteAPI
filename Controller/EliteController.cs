using EliteService.EliteServiceManager;
using EliteService.EliteServiceManager.Models.DTO;
using EliteService.EliteServiceManager.Models.Request;
using EliteService.EliteServiceManager.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace EliteService.Controller;

[Route($"api/elite/v1")]

public class EliteController(EliteServiceManager.EliteServiceManager eliteService, ILogger<EliteController> logger, DynamoDbService dynamoDbService) : ControllerBase
{
    private readonly DynamoDbService _dynamoDbService = dynamoDbService;
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

    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        var user = await _dynamoDbService.GetUserAsync(username);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }
    
    // ✅ GET: /api/elite/v1 (Get all users)
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dynamoDbService.GetUsersAsync();
        return Ok(users);
    }

    // ✅ POST: /api/elite/v1 (Create a new user)
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Email))
        {
            return BadRequest(new { message = "Username, Password, and Email are required" });
        }

        await _dynamoDbService.CreateUserAsync(user);
        return Ok(new { message = "User created successfully", user });
    }
}
