using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analytics
{
    internal class AnalyticsDTO
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Device { get; set; }
        public float Battery { get; set; }
        public float AverageHumidity { get; set; }
        public float AverageTemperature { get; set; }
    }
}
