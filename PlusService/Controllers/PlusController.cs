using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using PlusService.Models;
using Monitoring;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using System.Diagnostics;

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
            using (var activity = Monitoring.Monitoring.ActivitySource.StartActivity("Addition post"))
            {
                var result = numbers.Sum();
                Monitoring.Monitoring.Log.Debug($"Started activity: {activity?.Kind}, ID: {activity?.Id}, ParentID: {activity?.ParentId}");
                Monitoring.Monitoring.Log.Debug("Entered Add Method");
    
                var calculationHistory = new CalculationHistory
                {
                    Id = Guid.NewGuid(),
                    Operation = "Addition",
                    Expression = string.Join(" + ", numbers),
                    Result = result
                };

                var jsonString = JsonSerializer.Serialize(calculationHistory);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                // _ is to explicitily discard the task returned by the async method without awaiting it. Fire and forget approach.
                //Essentially supresses the warning that im not awaiting an async task.
                _ = LogToHistoryServiceAsync(content);


                return Ok(result);
            }
        }

        private async Task LogToHistoryServiceAsync(HttpContent content)
        {
            try
            {
                var client = _clientFactory.CreateClient("HistoryClient");
                Monitoring.Monitoring.Log.Debug("Entered Log To History Service Async");
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
            }
        }


    }
}