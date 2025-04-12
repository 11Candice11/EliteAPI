using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using EliteService.EliteServiceManager;
using EliteService.EliteServiceManager.Models.DTO;
using EliteService.EliteServiceManager.Models.Request;
using EliteService.EliteServiceManager.Models.Response;
using EliteService.EliteServiceManager.Services;

[Route("api/elite/v1")]
[ApiController]
public class EliteController : ControllerBase
{
    private readonly ILogger<EliteController> _logger;
    private readonly DynamoDbService _dynamoDbService;
    private readonly IEliteServiceManager _eliteServceManager;
    private readonly ClientService _clientService;

    public EliteController(DynamoDbService dynamoDbService, ILogger<EliteController> logger, IEliteServiceManager eliteServceManager, ClientService clientService)
    {
        _dynamoDbService = dynamoDbService;
        _logger = logger;
        _eliteServceManager = eliteServceManager;
        _clientService = clientService;
    }

    /// <summary>
    /// Login endpoint
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Invalid user input.");
        }

        bool isValid = await _dynamoDbService.VerifyUserAsync(user.Username, user.Password, user.IDNumber);

        return isValid
            ? Ok(new { Message = "Login successful!" })
            : Unauthorized(new { Message = "Invalid username or password." });
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _dynamoDbService.GetUsersAsync();

        return users.Count > 0 ? Ok(users) : NoContent();
    }

    /// <summary>
    /// Retrieve a user by username
    /// </summary>
    [HttpGet("{username}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    /// Update user ID Number
    /// </summary>
    [HttpPut("update-user-id")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUserEmail([FromBody] UpdateIDNumberRequest request)
    {
        if (string.IsNullOrEmpty(request.IDNumber) || string.IsNullOrEmpty(request.NewIDNumber))
        {
            return BadRequest("Username and new email are required.");
        }

        var success = await _dynamoDbService.UpdateUserIDNumberAsync(request.IDNumber, request.NewIDNumber);
        return success ? Ok("Email updated successfully.") : StatusCode(500, "Failed to update email.");
    }

    /// <summary>
    /// Delete a user by username
    /// </summary>
    [HttpDelete("delete-user/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteUser(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required.");
        }

        var success = await _dynamoDbService.DeleteUserAsync(username);
        return success ? Ok("User deleted successfully.") : StatusCode(500, "Failed to delete user.");
    }


    // </summary>
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

        var responseModel = await _eliteServceManager.GetClientProfile(request);

        return responseModel != null ? Ok(responseModel) : NoContent();
    }

    [HttpPost("get-clients")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetClients([FromBody] ConsultantRequest request)
    {
        try
        {
            var clients = await _dynamoDbService.GetClientsByConsultant(request.ConsultantIDNumber);
            return Ok(clients);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("client-data/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    
    [HttpGet("client/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetClient(string clientId)
    {
        try
        {
            var client = await _dynamoDbService.GetClientAsync(clientId);
            return Ok(client);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    [HttpPost("save-client")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClientProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SaveClient([FromBody] ClientProfileResponse clientData)
    {
        var result = await _clientService.SaveClient(clientData);

        if (result)
            return Ok(new { Message = "Client data saved successfully" });

        return StatusCode(500, "Failed to save client data");
    }

    [HttpGet("get-client-info/{idNumber}")]
    public async Task<IActionResult> GetClientInfo(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber))
            return BadRequest("ID Number cannot be empty");

        var clientData = await _clientService.GetClientInfo(idNumber);

        if (clientData == null)
            return NotFound($"Client with ID Number '{idNumber}' not found");

        return Ok(clientData);
    }
    
    [HttpPost("clients")]
    public async Task<IActionResult> AddClient([FromBody] Client client)
    {
        try
        {
            // Check if the client object itself is null
            if (client == null)
            {
                return BadRequest("Client object cannot be null.");
            }
    
            // Ensure ConsultantIDNumber is provided
            if (string.IsNullOrEmpty(client.IDNumber))
            {
                return BadRequest("Consultant ID Number is required.");
            }
    
            // Check if _dynamoDbService is null
            if (_dynamoDbService == null)
            {
                return StatusCode(500, "Internal Server Error: Database service is not initialized.");
            }
            
            // Call DynamoDB service
            await _dynamoDbService.AddClient(client);
            return StatusCode(201, client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception in AddClient: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("client-data")]
    public async Task<IActionResult> AddClientData([FromBody] ClientData clientData)
    {
        try
        {
            await _dynamoDbService.AddClientData(clientData);
            return StatusCode(201, clientData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
