using Microsoft.AspNetCore.Mvc;
using MinusService.Models;
using System.Text.Json;
using System.Text;

namespace MinusService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MinusController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;

        public MinusController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        [HttpPost]
        public IActionResult Substract([FromBody] List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                return BadRequest("No numbers provided.");

            var result = numbers.First();
            for (int i = 1; i < numbers.Count; i++)
            {
                result -= numbers[i];
            }

            var calculationHistory = new CalculationHistory
            {
                Id = Guid.NewGuid(),
                Operation = "Subtraction",
                Expression = string.Join(" - ", numbers),
                Result = result
            };

            var jsonString = JsonSerializer.Serialize(calculationHistory);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            _ = LogToHistoryServiceAsync(content);

            return Ok(result);
        }

        private async Task LogToHistoryServiceAsync(HttpContent content)
        {
            try
            {
                var client = _clientFactory.CreateClient("HistoryClient");
                await client.PostAsync("", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't insert it into history db, cause of error: {ex}");
                // implement further logging or error handling here if needed.
            }
        }
    }
}