using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EliteAPI.Controller;

[ApiController]
[Route("[controller]")]
public class PdfProxyController : ControllerBase
{
    private readonly PdfRetrievalService _pdfService;

    public PdfProxyController(PdfRetrievalService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpGet("fetch")]
    public async Task<IActionResult> Fetch([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("Missing 'url' query parameter.");
        }

        try
        {
            // Retrieve the PDF as bytes
            var pdfBytes = await _pdfService.GetPdfBytesAsync(url);

            // Return the PDF with the correct Content-Type
            return File(pdfBytes, "application/pdf");
        }
        catch (HttpRequestException ex)
        {
            // 502 = Bad Gateway
            return StatusCode(502, $"Failed to fetch PDF from upstream: {ex.Message}");
        }
    }
}