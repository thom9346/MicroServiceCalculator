using SharedModels;

namespace HistoryService.Models
{
    public class CalculationHistoryConverter : IConverter<CalculationHistory, CalculationHistoryDto>
    {
        public CalculationHistory Convert(CalculationHistoryDto sharedModel)
        {
            return new CalculationHistory
            {
                Id = sharedModel.Id,
                Expression = sharedModel.Expression,
                Operation = sharedModel.Operation,
                Result = sharedModel.Result,
            };
        }

        public CalculationHistoryDto Convert(CalculationHistory hiddenModel)
        {
            return new CalculationHistoryDto
            {
                Id = hiddenModel.Id,
                Expression = hiddenModel.Expression,
                Operation = hiddenModel.Operation,
                Result = hiddenModel.Result,
            };
        }
    }
}
