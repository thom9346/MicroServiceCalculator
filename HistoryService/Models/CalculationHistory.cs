﻿using System.ComponentModel.DataAnnotations;

namespace HistoryService.Models
{
    public class CalculationHistory
    {
        [Key]
        public Guid Id { get; set; }
        public string Operation { get; set; }
        public string Expression { get; set; }
        public double Result { get; set; }

    }
}
