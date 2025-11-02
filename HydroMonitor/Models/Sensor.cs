using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitor.Models
{
    public class Sensor
    {
        //public Guid sensorId { get; } //this was a bad idea, just use an int
        [PrimaryKey, AutoIncrement] //tag for the sqlite library to do its thing
        public int SensorId { get; set;}
        public String Name { get; set; }
        public String IpAddress { get; set; }

        //[ForeignKey(typeof(SensorType))]
        public int SensorTypeId { get; set; }
        [Ignore]
        public  SensorType SensorType { get; set; }
        public bool enabled { get; set; }



        public Sensor()
        {
            Name = "";
            IpAddress = "";
            SensorType = new SensorType(MeasurementType.Percentage);
            enabled = true;
        }

    }
}
