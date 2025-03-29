using System.Net.Http;
using System.Threading.Tasks;

public class PdfRetrievalService
{
    private readonly HttpClient _httpClient;

    // The HttpClient is injected via DI in Program.cs or Startup.cs
    public PdfRetrievalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GetPdfBytesAsync(string url)
    {
        // Fetch the PDF from the provided URL
        var response = await _httpClient.GetAsync(url);

        // Throw an exception if the response is not successful
        response.EnsureSuccessStatusCode();

        // Return the raw PDF bytes
        return await response.Content.ReadAsByteArrayAsync();
    }
}