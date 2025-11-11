using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HydroMonitor.Models
{
    /// <summary>
    /// Represents the type of sensor and what it measures. 
    /// For example, a humidity sensor might have the typeName Humidity and the measurementType Percentage.
    /// </summary>
    
    public enum MeasurementType
    {
        Percentage,
        Double,
        Integer
    }
    public class SensorType
    {
        [PrimaryKey, AutoIncrement]
        public int typeId { get; set; }
        public String Name { get; set; }
        public MeasurementType measurementType { get; set; }


        public SensorType()
        {
            typeId = 0;
            Name = "";

        }


        public SensorType(MeasurementType type)
        {
            typeId = 0;
            Name = "";
            this.measurementType = type;
        }

    }
}
