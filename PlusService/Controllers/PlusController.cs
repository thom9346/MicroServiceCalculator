using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using PlusService.Models;
using Monitoring;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace PlusService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlusController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;

        public PlusController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        [HttpPost] 
        public IActionResult Add([FromBody] List<int> numbers)
        {
            int result;

            using (var activity = Monitoring.Monitoring.ActivitySource.StartActivity("POST request at the /Plus/ endpoint"))
            {
                Monitoring.Monitoring.Log.Debug("Entered Add Method In /Plus/ endpoint");

                // Start a span for the calculation
                using (var calculationSpan = Monitoring.Monitoring.ActivitySource.StartActivity("Making the calculation", ActivityKind.Internal, activity.Context))
                {
                    result = numbers.Sum();
                    calculationSpan.SetTag("items.count", numbers.Count);
                    calculationSpan.SetTag("result", result);
                }
                var expression = string.Join(" + ", numbers);

                Monitoring.Monitoring.Log.Information($"Expression :{expression} had the following result: {result}");

                var calculationHistory = new CalculationHistory
                {
                    Id = Guid.NewGuid(),
                    Operation = "Addition",
                    Expression = expression,
                    Result = result
                };

                var jsonString = JsonSerializer.Serialize(calculationHistory);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                // Start a span for the HTTP call to HistoryService
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
                        clientSpan.SetTag("status", "Error during HTTP call to HistoryService from PlusService");
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
                Monitoring.Monitoring.Log.Debug("Entered Log To History Service Async In /Plus/");
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