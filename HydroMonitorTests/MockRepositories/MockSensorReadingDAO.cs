using HydroMonitor.Models;
using HydroMonitor.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitorTests.MockRepositories
{
    public class MockSensorReadingDAO : SensorReadingDAO
    {
        List<SensorReading> savedEntries;


        public bool Save(SensorReading reading)
        {
          
            savedEntries.Add(reading);
            return true;
        }

        public  List<SensorReading> Get(int sensorId)
        {
            return savedEntries;
        }


    }
}
