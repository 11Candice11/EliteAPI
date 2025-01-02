using System.Net.Http.Headers;
using System.Text;
using EliteAPI.Models.Request;
using EliteAPI.Models.Response;
using Newtonsoft.Json;

namespace EliteAPI.EliteAPI;

public class EliteManager(ILogger<EliteManager> logger) : IEliteManager
{
        private readonly ILogger<EliteManager> _logger = logger;
    const string BASE_URL_AUTHENTICATION = "https://morebo.elitewealth.biz/restApiAuthentication";
    const string API_KEY = "Q3NuWXrjNnB8szjXd7y8bt2TR4-DgFdkKQnj45jKc8xmMVbY6frwRkZXmYsqYrCcwQL";
    const string USER_NAME = "QGFgzP49w4DuqJmA"; //"WebServiceDemo";
    private const string PASSWORD = "QrSzh93D4wNR4BEs"; //"Dem@nstrate13";

    const string BASE_URL_DATA = "https://morebo.elitewealth.biz";

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

                // Read the raw JSON response
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var responseModel = JsonConvert.DeserializeObject<ClientProfileResponse>(jsonResponse);
                // Print the raw response
                Console.WriteLine("Raw JSON Response:");
                Console.WriteLine(jsonResponse);
                return responseModel;
            }
            catch (Exception ex)
            {
                // Handle any errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ClientProfileResponse();
            }
    }

    //
    // public async Task<LoadClientResponse> LoadClient(LoadClientRequest clientRequest)
    // {
    //     var token = await GetToken();
    //     var httpClient = new HttpClient()
    //     {
    //         BaseAddress = new Uri(BASE_URL_DATA + "/api/EntityCardChangeEntity/Search")
    //     };
    //     httpClient.DefaultRequestHeaders.Authorization =
    //         new AuthenticationHeaderValue("Bearer", token.AccessToken);
    //     httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //
    //     var request = new LoadClientRequest()
    //     {
    //     };
    //
    //     // Serialize the request to JSON
    //     var jsonRequest = JsonConvert.SerializeObject(request);
    //     var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
    //
    //     try
    //     {
    //         // Make the POST request
    //         var response = httpClient.PostAsync("", httpContent).Result;
    //
    //         // Read the raw JSON response
    //         var jsonResponse = response.Content.ReadAsStringAsync().Result;
    //         var responseModel = JsonConvert.DeserializeObject<ClientProfileResponse>(jsonResponse);
    //         // Print the raw response
    //         Console.WriteLine("Raw JSON Response:");
    //         Console.WriteLine(jsonResponse);
    //         return responseModel;
    //     }
    //     catch (Exception ex)
    //     {
    //         // Handle any errors
    //         Console.WriteLine($"An error occurred: {ex.Message}");
    //         return new ClientProfileResponse();
    //     }
    // }
    //
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
}