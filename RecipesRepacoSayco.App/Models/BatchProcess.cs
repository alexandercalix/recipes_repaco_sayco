using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesRepacoSayco.App.Models
{
    public class BatchProcess
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string ProductName { get; set; }

        public float? Setpoint1 { get; set; }
        public float? ActualValue1 { get; set; }

        public float? Setpoint2 { get; set; }
        public float? ActualValue2 { get; set; }

        public float? Setpoint3 { get; set; }
        public float? ActualValue3 { get; set; }

        public float? Setpoint4 { get; set; }
        public float? ActualValue4 { get; set; }
    }
}