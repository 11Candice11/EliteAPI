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

        bool isValid = await _dynamoDbService.VerifyUserAsync(user.Username, user.Password);

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
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.IDNumber))
        {
            return BadRequest(new { message = "Username, Password, and IDNumber are required" });
        }

        await _dynamoDbService.CreateUserAsync(user);
        return Ok(new { message = "User created successfully", user });
    }
    /// <summary>
    /// Update user IDNumber
    /// </summary>
    [HttpPut("update-user-id")]
    public async Task<IActionResult> UpdateUserIDNumber([FromBody] UpdateIDNumberRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.IDNumber) || string.IsNullOrEmpty(request.NewIDNumber))
        {
            return BadRequest("Username and new IDNumber are required.");
        }

        var success = await _dynamoDbService.UpdateUserIDNumberAsync(request.IDNumber, request.NewIDNumber);
        return success ? Ok("IDNumber updated successfully.") : StatusCode(500, "Failed to update IDNumber.");
    }

    /// <summary>
    /// Delete a user by username
    /// </summary>
    [HttpDelete("delete-user/{username}")]
    public async Task<IActionResult> DeleteUser(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required.");
        }

        var success = await _dynamoDbService.DeleteUserAsync(username);
        return success ? Ok("User deleted successfully.") : StatusCode(500, "Failed to delete user.");
    }
    
    [HttpGet("clients")]
    public async Task<IActionResult> GetClients()
    {
        try
        {
            var idNumber = User.Claims.FirstOrDefault(c => c.Type == "idNumber")?.Value;
            if (idNumber == null) return Unauthorized();
            var clients = await _dynamoDbService.GetClientsByConsultant(idNumber);
            return Ok(clients);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("client-data/{clientId}")]
    public async Task<IActionResult> GetClientData(string clientId)
    {
        try
        {
            var clientData = await _dynamoDbService.GetClientData(clientId);
            return Ok(clientData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("clients")]
    public async Task<IActionResult> AddClient([FromBody] Client client)
    {
        try
        {
            var consultantIDNumber = User.Claims.FirstOrDefault(c => c.Type == "idNumber")?.Value;
            if (consultantIDNumber == null) return Unauthorized();
            
            client.ConsultantIDNumber = consultantIDNumber;
            await _dynamoDbService.AddClient(client);
            return StatusCode(201, client);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("client-data")]
    public async Task<IActionResult> AddClientData([FromBody] ClientData clientData)
    {
        try
        {
            var consultantIDNumber = User.Claims.FirstOrDefault(c => c.Type == "idNumber")?.Value;
            if (consultantIDNumber == null) return Unauthorized();
            
            clientData.ConsultantIDNumber = consultantIDNumber;
            await _dynamoDbService.AddClientData(clientData);
            return StatusCode(201, clientData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
