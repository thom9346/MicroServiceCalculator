
using HistoryService.Data;
using HistoryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monitoring;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using SharedModels;
using System.Diagnostics;

namespace HistoryService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IRepository<CalculationHistory> _historyRepository;
        private readonly IConverter<CalculationHistory, CalculationHistoryDto> _converter;

        public HistoryController(IRepository<CalculationHistory> repo, IConverter<CalculationHistory, CalculationHistoryDto> converter)
        {
            _historyRepository = repo;
            _converter = converter;
        }

        [HttpPost]
        public IActionResult Post([FromBody] CalculationHistoryDto calculationDto)
        {
            var propagator = new TraceContextPropagator();
            var parentContext = propagator.Extract(default, Request.Headers, (headers, key) =>
            {
                if (headers.TryGetValue(key, out var values) && values.Count > 0)
                {
                    return new List<string> { values[0] };
                }
                return new List<string>();
            });

            Baggage.Current = parentContext.Baggage;
            using var activity = Monitoring.Monitoring.ActivitySource.StartActivity("Entered Post In /History/ endpoint", ActivityKind.Server, parentContext.ActivityContext);


            if (calculationDto == null)
            {
                Monitoring.Monitoring.Log.Warning("calculationDto is null, returned BadRequest");
                return BadRequest();
            }
      
            var calculation = _converter.Convert(calculationDto);
            
            var newCalculation = _historyRepository.Add(calculation);

            return CreatedAtRoute("GetHistory", new { id = newCalculation.Id }, _converter.Convert(newCalculation));
        }

        [HttpGet]
        public IEnumerable<CalculationHistoryDto> Get()
        {
            var calculationHistoryDtoList = new List<CalculationHistoryDto>();
            Monitoring.Monitoring.Log.Debug("Entered Get() Method in /History/");

            using (var activity = Monitoring.Monitoring.ActivitySource.StartActivity("Entered Get In /History/ endpoint", ActivityKind.Internal))
            {
                foreach (var calculation in _historyRepository.GetAll())
                {
                    var calculationHistoryDto = _converter.Convert(calculation);
                    calculationHistoryDtoList.Add(calculationHistoryDto);
                };
            }
            
            return calculationHistoryDtoList;
        }

[HttpGet("{id}", Name="GetHistory")]
        public IActionResult Get(Guid id)
        {
            var item = _historyRepository.Get(id);
            if(item == null)
            {
                return NotFound();
            }
            var calculationHistoryDto = _converter.Convert(item);
            return new ObjectResult(calculationHistoryDto);
        }
    }
}