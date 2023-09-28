namespace SharedModels
{
    public class CalculationHistoryDto
    {
        public Guid Id { get; set; }
        public string Operation { get; set; }
        public string Expression { get; set; }
        public double Result { get; set; }

    }
}