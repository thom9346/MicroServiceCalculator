using Microsoft.AspNetCore.Mvc;
using MinusService.Models;
using System.Text.Json;
using System.Text;
using Monitoring;
using System.Diagnostics;
using System.Linq.Expressions;
using OpenTelemetry.Trace;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

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
            int result;
            using (var activity = Monitoring.Monitoring.ActivitySource.StartActivity("POST request at the /Minus/ endpoint"))
            {
                Monitoring.Monitoring.Log.Debug("Entered Substract Method In /Minus/ endpoint");

                using (var calculationSpan = Monitoring.Monitoring.ActivitySource.StartActivity("Making the calculation", ActivityKind.Internal, activity.Context))
                {
                    result = numbers.First();
                    for (int i = 1; i < numbers.Count; i++)
                    {
                        result -= numbers[i];
                    }
                    calculationSpan.SetTag("items.count", numbers.Count);
                    calculationSpan.SetTag("result", result);
                }

                if (numbers == null || numbers.Count == 0)
                {
                    Monitoring.Monitoring.Log.Warning("No numbers provided for Substract at /Minus endpoint");
                    return BadRequest("No numbers provided.");
                }
                var expression = string.Join(" - ", numbers);

                Monitoring.Monitoring.Log.Information($"Expression :{expression} had the following result: {result}");

                var calculationHistory = new CalculationHistory
                {
                    Id = Guid.NewGuid(),
                    Operation = "Subtraction",
                    Expression = expression,
                    Result = result
                };

                var jsonString = JsonSerializer.Serialize(calculationHistory);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                using (var clientSpan = Monitoring.Monitoring.ActivitySource.StartActivity("Async HTTP call to HistoryService", ActivityKind.Client, activity.Context))
                {
                    try
                    {
                        _ = LogToHistoryServiceAsync(content);
                    }
                    catch (Exception ex)
                    {
                        Monitoring.Monitoring.Log.Error($"Couldn't insert it into history db, cause of error: {ex}");
                        activity.RecordException(ex);
                        clientSpan.SetTag("status", "Error during HTTP call to HistoryService from MinusService");
                    }
                }
                return Ok(result);
            }
        }

        private async Task LogToHistoryServiceAsync(HttpContent content)
        {
            try
            {
                var client = _clientFactory.CreateClient("HistoryClient");
                Monitoring.Monitoring.Log.Debug("Entered Log To History Service Async In /Minus/");
                var currentActivity = Activity.Current;
                if (currentActivity != null)
                {
                    var propagationContext = new PropagationContext(currentActivity.Context, Baggage.Current);
                    var propagator = new TraceContextPropagator();
                    propagator.Inject(propagationContext, client.DefaultRequestHeaders, (headers, key, value) =>
                    {
                        headers.Add(key, value);
                    });
                }

                await client.PostAsync("", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't insert it into history db, cause of error: {ex}");
                Monitoring.Monitoring.Log.Error($"Couldn't insert it into history db, cause of error: {ex}");
            }
        }
    }
}