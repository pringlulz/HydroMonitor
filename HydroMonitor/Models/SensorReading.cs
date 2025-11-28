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
        [PrimaryKey, AutoIncrement] //sqlite-net kinda sucks and doesn't support composite keys. So we gotta do this.
        public long readingId { get; set; }

        [Indexed(Name = "CompositeKey", Order = 1, Unique = true)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [Indexed(Name = "CompositeKey", Order = 2, Unique = true)]
        public int SensorId { get; set; }


        public MeasurementType Type { get; set; }
        public String? rawValue { get; set; }
        
        public double doubleValue()
        {
            return Convert.ToDouble(rawValue);
        }
    }
}
