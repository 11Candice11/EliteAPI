using System.Net.Http.Headers;
using System.Text;
using EliteService.Controller;
using EliteService.EliteServiceManager.Models.DTO;
using EliteService.EliteServiceManager.Models.Request;
using EliteService.EliteServiceManager.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace EliteService.EliteServiceManager;

public class EliteServiceManager : IEliteServiceManager
{
    private readonly string _connectionString;
    private readonly ILogger<EliteServiceManager> _logger;

    const string BASE_URL_AUTHENTICATION = "https://morebo.elitewealth.biz/restApiAuthentication";
    const string API_KEY = "Q3NuWXrjNnB8szjXd7y8bt2TR4-DgFdkKQnj45jKc8xmMVbY6frwRkZXmYsqYrCcwQL";
    const string USER_NAME = "QGFgzP49w4DuqJmA"; // "WebServiceDemo";
    private const string PASSWORD = "QrSzh93D4wNR4BEs"; // "Dem@nstrate13";
    const string BASE_URL_DATA = "https://morebo.elitewealth.biz";

    public EliteServiceManager(ILogger<EliteServiceManager> logger, IConfiguration configuration)
    {
        _logger = logger;

        // Retrieve the connection string from appsettings.json
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<bool> VerifyUserAsync(string username, string password)
    {
        try
        {
            Console.WriteLine($"[DEBUG] VerifyUserAsync called with Username: {username}");

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("[DEBUG] Database connection opened.");

                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    Console.WriteLine($"[DEBUG] Query: {query}");
                    Console.WriteLine($"[DEBUG] Parameters: Username={username}, Password={password}");

                    int count = (int)await command.ExecuteScalarAsync();
                    Console.WriteLine($"[DEBUG] Query executed. Matching users found: {count}");

                    return count > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception in VerifyUserAsync: {ex.Message}");
            return false;
        }
    }
    
    // api/entitySummary
    public async Task<ClientProfileResponse> GetClientProfile(ClientProfileRequest clientProfileRequest)
    {
        _logger.LogInformation("Getting client profile at: {time}", DateTimeOffset.Now);
        var token = await GetToken();
        
        var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BASE_URL_DATA + "/restApiData/RestApiMoreboPortfolio")
            };
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new ClientProfileRequest
            {
                TransactionDateStart = clientProfileRequest.TransactionDateStart,
                TransactionDateEnd = clientProfileRequest.TransactionDateEnd,
                TargetCurrencyL = clientProfileRequest.TargetCurrencyL,
                ValueDates = clientProfileRequest.ValueDates,
                InputEntityModels = clientProfileRequest.InputEntityModels
            };

            // Serialize the request to JSON
            var jsonRequest = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                // Make the POST request
                var response = httpClient.PostAsync("", httpContent).Result;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                
                var responseObject = JsonSerializer.Deserialize<ClientProfileResponse>(jsonResponse);
                
                // Print the raw response
                return responseObject;
            }
            catch (Exception ex)
            {
                // Handle any errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ClientProfileResponse();
            }
    }
    private async Task<AuthenticationResponseModel> GetToken()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(BASE_URL_AUTHENTICATION + "/RestApiAccessToken")
        };

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("api-key", API_KEY);

        var authenticationRequestModel = new AuthenticationRequestModel
        {
            Username = USER_NAME,
            Password = PASSWORD
        };

        var jsonRequest = JsonConvert.SerializeObject(authenticationRequestModel);
        var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = httpClient.PostAsync("", httpContent).Result;
        var jsonResponse = response.Content.ReadAsStringAsync().Result;

        var authenticationResponseModel = JsonConvert.DeserializeObject<AuthenticationResponseModel>(jsonResponse);
        Console.WriteLine($"Received access token '{authenticationResponseModel.AccessToken}'");

        return authenticationResponseModel;
    }
    
    private string HashPassword(string password)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}