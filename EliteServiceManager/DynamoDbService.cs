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

public class DynamoDbService
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly DynamoDBContext _dbContext;
    private readonly string _tableName = "Users"; // Change this to your actual table name

    public DynamoDbService(IConfiguration configuration)
    {
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? configuration["AWS:AccessKey"];
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? configuration["AWS:SecretKey"];
        var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? configuration["AWS:Region"];

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(region))
        {
            throw new ArgumentNullException("AWS credentials or region are missing.");
        }

        Console.WriteLine($"Initializing DynamoDB with Region: {region}");

        var awsCredentials = new BasicAWSCredentials(accessKey, secretKey);
        var amazonDynamoDbConfig = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };

        _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, amazonDynamoDbConfig);
        _dbContext = new DynamoDBContext(_dynamoDbClient);
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
                { "Email", new AttributeValue { S = user.Email ?? string.Empty } }
            }
        };

        var response = await _dynamoDbClient.PutItemAsync(request);
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
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
            Email = response.Item.ContainsKey("Email") ? response.Item["Email"].S : null
        };
    }

    /// <summary>
    /// Updates an existing user's email in the DynamoDB table.
    /// </summary>
    public async Task<bool> UpdateUserEmailAsync(string username, string newEmail)
    {
        if (_dynamoDbClient == null)
        {
            throw new NullReferenceException("DynamoDB client is not initialized.");
        }

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newEmail))
        {
            throw new ArgumentNullException("Username and new email cannot be null.");
        }

        Console.WriteLine($"Updating email for user: {username}");

        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { { "Username", new AttributeValue { S = username } } },
            AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
            {
                { "Email", new AttributeValueUpdate { Action = AttributeAction.PUT, Value = new AttributeValue { S = newEmail } } }
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
}
