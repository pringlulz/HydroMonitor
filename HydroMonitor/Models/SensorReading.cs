using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitor.Models
{
    public class SensorReading
    {
        [PrimaryKey]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        MeasurementType Type;
        public String? rawValue { get; set; }
        
        public double doubleValue()
        {
            return Convert.ToDouble(rawValue);
        }
    }
}
