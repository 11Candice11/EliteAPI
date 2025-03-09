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
    private readonly string _clientsTable = "Clients";
    private readonly string _clientDataTable = "ClientData";

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
        _dbContext = new DynamoDBContext(_dynamoDbClient);
    }

    public async Task<bool> VerifyUserAsync(string username, string password, string IDNumber)
    {
        try
        {
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
                return false;
            }

            string storedPassword = response.Item["Password"].S;

            bool isMatch = storedPassword == password; // Ensure you use hashing for security!

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
            new ScanCondition("ClientID", ScanOperator.Equal, clientId)
        };
        return await _dbContext.ScanAsync<ClientData>(conditions).GetRemainingAsync();
    }

    /// <summary>
    /// Retrieves a single client's information by ClientID.
    /// </summary>
    /// <param name="clientID">The unique identifier of the client.</param>
    /// <returns>The client information if found, otherwise null.</returns>
    public async Task<Client> GetClientAsync(string clientID)
    {
        try
        {
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentNullException(nameof(clientID), "ClientID cannot be null or empty.");
            }

            var request = new GetItemRequest
            {
                TableName = _clientsTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "ClientID", new AttributeValue { S = clientID } }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
            {
                return null;
            } 
            
            // Map response to Client model
            return new Client
            {
                ClientID = response.Item["ClientID"].S,
                FirstNames = response.Item.ContainsKey("FirstNames") ? response.Item["FirstNames"].S : null,
                Surname = response.Item.ContainsKey("Surname") ? response.Item["Surname"].S : null,
                RegisteredName = response.Item.ContainsKey("RegisteredName") ? response.Item["RegisteredName"].S : null,
                Title = response.Item.ContainsKey("Title") ? response.Item["Title"].S : null,
                Nickname = response.Item.ContainsKey("Nickname") ? response.Item["Nickname"].S : null,
                AdvisorName = response.Item.ContainsKey("AdvisorName") ? response.Item["AdvisorName"].S : null,
                Email = response.Item.ContainsKey("Email") ? response.Item["Email"].S : null,
                CellPhoneNumber = response.Item.ContainsKey("CellPhoneNumber") ? response.Item["CellPhoneNumber"].S : null,
                ConsultantIDNumber = response.Item.ContainsKey("ConsultantIDNumber") ? response.Item["ConsultantIDNumber"].S : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception in GetClientAsync: {ex.Message}");
            throw;
        }
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
