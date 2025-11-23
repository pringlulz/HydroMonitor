using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitor.Models
{
    public class SensorLocation
    {

        

        [PrimaryKey]
        public int SensorId { get; set; }
        public double locationX { get; set; }
        public double locationY { get; set; }
        public double locationZ { get; set; }


        public Tuple<double, double, double> GetCoordinates()
        {
            return Tuple.Create(locationX,locationY,locationZ);
            ;
        }
        public void SetCoordinates(Tuple<double,double,double> coords)
        {
            locationX = coords.Item1;
            locationY = coords.Item2;
            locationZ = coords.Item3;

        }

    }
}
