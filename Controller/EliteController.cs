using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/elite/v1")]
[ApiController]
public class EliteController : ControllerBase
{
    private readonly ILogger<EliteController> _logger;
    private readonly DynamoDbService _dynamoDbService;

    public EliteController(DynamoDbService dynamoDbService, ILogger<EliteController> logger)
    {
        _dynamoDbService = dynamoDbService;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Invalid user input.");
        }

        bool isValid = await _dynamoDbService.VerifyUserAsync(user.Username, user.Password);

        return isValid ? Ok(new { Message = "Login successful!" }) : Unauthorized(new { Message = "Invalid username or password." });
    }

    /// <summary>
    /// Create a new user in DynamoDB
    /// </summary>
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username))
        {
            return BadRequest("User data is required.");
        }

        var success = await _dynamoDbService.CreateUserAsync(user);

        return success ? Ok($"User {user.Username} created successfully.") : StatusCode(500, "Failed to create user.");
    }

    /// <summary>
    /// Get all users from DynamoDB
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _dynamoDbService.GetAllUsersAsync();

        return users.Count > 0 ? Ok(users) : NoContent();
    }

    /// <summary>
    /// Retrieve a user by username
    /// </summary>
    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required.");
        }

        var user = await _dynamoDbService.GetUserAsync(username);

        return user != null ? Ok(user) : NotFound("User not found.");
    }

    /// <summary>
    /// Update user email
    /// </summary>
    [HttpPut("update-user-email")]
    public async Task<IActionResult> UpdateUserEmail([FromBody] UpdateEmailRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.NewEmail))
        {
            return BadRequest("Username and new email are required.");
        }

        var success = await _dynamoDbService.UpdateUserEmailAsync(request.Username, request.NewEmail);
        return success ? Ok("Email updated successfully.") : StatusCode(500, "Failed to update email.");
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

    /// <summary>
    /// Retrieve client profile
    /// </summary>
    [HttpPost("get-client-profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ClientProfileResponse>> GetClientProfile([FromBody] ClientProfileRequest request)
    {
        if (request == null)
        {
            return BadRequest("Client profile request cannot be null.");
        }

        var responseModel = await _dynamoDbService.GetClientProfile(request);

        return responseModel != null ? Ok(responseModel) : NoContent();
    }
}
