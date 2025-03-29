using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EliteService.EliteServiceManager;
using EliteService.EliteServiceManager.Models.Request;

namespace EliteAPI.Controller
{
    [ApiController]
    [Route("api/ratings")]
    public class PortfolioRatingsController : ControllerBase
    {
        private readonly DynamoDbService _ratingService;

        public PortfolioRatingsController(DynamoDbService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetRatingsForClient(string clientId)
        {
            var ratings = await _ratingService.GetRatingsForClientAsync(clientId);
            return Ok(ratings);
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveRating([FromBody] PortfolioRating rating)
        {
            if (rating == null || string.IsNullOrEmpty(rating.IsinNumber)) {
                return BadRequest("Invalid rating or ISIN");
            }

            await _ratingService.SaveRatingAsync(rating);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRatings()
        {
            var ratings = await _ratingService.GetAllRatingsAsync();
            return Ok(ratings);
        }
    }
}