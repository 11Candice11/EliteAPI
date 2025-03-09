using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EliteService.EliteServiceManager.Models.DTO;

public class DynamoDbService
{
    // private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    // private readonly DynamoDBContext _dbContext;
    private readonly IDynamoDBContext _dbContext;
    private readonly string _tableName = "Users"; // Change this to your actual table name
    // private readonly Table _clientsTable;
    // private readonly Table _clientDataTable;

    public DynamoDbService(IConfiguration configuration) {
        var accessKey = Environment.GetEnvironmentVariable("AWS__AccessKey", EnvironmentVariableTarget.Process);
        var secretKey = Environment.GetEnvironmentVariable("AWS__SecretKey", EnvironmentVariableTarget.Process);
        var region = Environment.GetEnvironmentVariable("AWS__Region", EnvironmentVariableTarget.Process);

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("AWS credentials are missing. Check Azure Configuration settings.");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var config = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };
        _dynamoDbClient = new AmazonDynamoDBClient(credentials, config);
    }

    public async Task<bool> VerifyUserAsync(string username, string password, string IDNumber)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Verifying user: {username}");
            Console.WriteLine($"[DEBUG] with password: {password}");
            Console.WriteLine($"[DEBUG] with ID: {IDNumber}");

            var request = new GetItemRequest
            {
                TableName = "Users", // Ensure this matches your DynamoDB table name
                Key = new Dictionary<string, AttributeValue>
                {
                    { "IDNumber", new AttributeValue { S = IDNumber } }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(request);

            if (response.Item == null || !response.Item.ContainsKey("Password"))
            {
                Console.WriteLine("[DEBUG] User not found or missing password field.");
                return false;
            }

            string storedPassword = response.Item["Password"].S;
            Console.WriteLine($"[DEBUG] Stored password for user {username}: {storedPassword}");

            bool isMatch = storedPassword == password; // Ensure you use hashing for security!
            Console.WriteLine($"[DEBUG] Password match result: {isMatch}");

            return isMatch;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception in VerifyUserAsync: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Creates a new user in the DynamoDB table.
    /// </summary>
    public async Task<bool> CreateUserAsync(User user)
    {
        if (_dynamoDbClient == null)
        {
            throw new NullReferenceException("DynamoDB client is not initialized.");
        }

        if (user == null || string.IsNullOrEmpty(user.Username))
        {
            throw new ArgumentNullException(nameof(user), "User object cannot be null or missing username.");
        }

        Console.WriteLine($"Creating user: {user.Username}");

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "Username", new AttributeValue { S = user.Username } },
                { "IDNumber", new AttributeValue { S = user.IDNumber } },
                { "Password", new AttributeValue { S = user.Password } }
            }
        };

        var response = await _dynamoDbClient.PutItemAsync(request);
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        if (_dynamoDbClient == null)
        {
            throw new NullReferenceException("DynamoDB client is not initialized.");
        }

        var scanRequest = new ScanRequest
        {
            TableName = "Users"
        };

        var response = await _dynamoDbClient.ScanAsync(scanRequest);

        if (response.Items == null || response.Items.Count == 0)
        {
            return new List<User>(); // Return empty list if no users found
        }

        return response.Items.Select(item => new User
        {
            Username = item.ContainsKey("Username") ? item["Username"].S : null,
            IDNumber = item.ContainsKey("IDNumber") ? item["IDNumber"].S : null,
            Password = item.ContainsKey("Password") ? item["Password"].S : null,
        }).ToList();
    }
    
    /// <summary>
    /// Retrieves a user from the DynamoDB table by username.
    /// </summary>
    public async Task<User> GetUserAsync(string username)
    {
        if (_dynamoDbClient == null)
        {
            throw new NullReferenceException("DynamoDB client is not initialized.");
        }

        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username), "Username cannot be null.");
        }

        Console.WriteLine($"Fetching user: {username}");

        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { { "Username", new AttributeValue { S = username } } }
        };

        var response = await _dynamoDbClient.GetItemAsync(request);
        if (!response.Item.Any())
        {
            return null;
        }

        return new User
        {
            Username = response.Item["Username"].S,
            IDNumber = response.Item.ContainsKey("IDNumber") ? response.Item["IDNumber"].S : null,
            Password = response.Item.ContainsKey("Password") ? response.Item["Password"].S : null,
        };
    }

    /// <summary>
    /// Updates an existing user's IDNumber in the DynamoDB table.
    /// </summary>
    public async Task<bool> UpdateUserIDNumberAsync(string idNumber, string newIDNumber)
    {
        if (_dynamoDbClient == null)
        {
            throw new NullReferenceException("DynamoDB client is not initialized.");
        }

        if (string.IsNullOrEmpty(idNumber) || string.IsNullOrEmpty(newIDNumber))
        {
            throw new ArgumentNullException("Username and new IDNumber cannot be null.");
        }

        Console.WriteLine($"Updating IDNumber for user: {idNumber}");

        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { { "IDNumber", new AttributeValue { S = idNumber } } },
            AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
            {
                { "NewIDNumber", new AttributeValueUpdate { Action = AttributeAction.PUT, Value = new AttributeValue { S = newIDNumber } } }
            }
        };

        var response = await _dynamoDbClient.UpdateItemAsync(request);
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }

    /// <summary>
    /// Deletes a user from the DynamoDB table.
    /// </summary>
    public async Task<bool> DeleteUserAsync(string username)
    {
        if (_dynamoDbClient == null)
        {
            throw new NullReferenceException("DynamoDB client is not initialized.");
        }

        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username), "Username cannot be null.");
        }

        Console.WriteLine($"Deleting user: {username}");

        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { { "Username", new AttributeValue { S = username } } }
        };

        var response = await _dynamoDbClient.DeleteItemAsync(request);
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }
    
    public async Task<List<Client>> GetClientsByConsultant(string consultantIDNumber)
    {
        var conditions = new List<ScanCondition>
        {
            new ScanCondition("ConsultantIDNumber", ScanOperator.Equal, consultantIDNumber)
        };
        return await _dbContext.ScanAsync<Client>(conditions).GetRemainingAsync();
    }
    
    
    // Fetch client data
    public async Task<List<ClientData>> GetClientData(string clientId)
    {
        var conditions = new List<ScanCondition>
        {
            new ScanCondition("IDNumber", ScanOperator.Equal, clientId)
        };
        return await _dbContext.ScanAsync<ClientData>(conditions).GetRemainingAsync();
    }

    // Add client
    public async Task AddClient(Client client)
    {
        await _dbContext.SaveAsync(client);
    }

    // Add client data
    public async Task AddClientData(ClientData clientData)
    {
        await _dbContext.SaveAsync(clientData);
    }
}
