
using HistoryService.Data;
using HistoryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;

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
            if (calculationDto == null)
            {
                return BadRequest();
            }
            var calculation = _converter.Convert(calculationDto);
            var newCalculation = _historyRepository.Add(calculation);

            return CreatedAtRoute("GetHistory", new { id = newCalculation.Id }, _converter.Convert(newCalculation));
        }

        [HttpGet]
        public IEnumerable<CalculationHistoryDto> Get()
        {
            var calculationHistoryDtoList = new List <CalculationHistoryDto>();
            foreach (var calculation in _historyRepository.GetAll())
            {
                var calculationHistoryDto = _converter.Convert(calculation);
                calculationHistoryDtoList.Add(calculationHistoryDto);
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