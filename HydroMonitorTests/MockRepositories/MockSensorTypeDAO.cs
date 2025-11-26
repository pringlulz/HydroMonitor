using HydroMonitor.Models;
using HydroMonitor.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HydroMonitorTests.MockRepositories
{
    internal class MockSensorTypeDAO : SensorTypeDAO
    {
        public MockSensorTypeDAO()
        {

        }

        public SensorType Load(String name)
        { //load by name instead of typeId
            SensorType t = new SensorType();
            t.Name = name;
            t.typeId = 1;
            t.measurementType = MeasurementType.Percentage;
            return t;
        }


        public SensorType Load(int pk)
        {
            SensorType t = new SensorType();
            t.Name = "Humidity";
            t.typeId = pk;
            t.measurementType = MeasurementType.Percentage;
            return t;
        }

        public List<SensorType> Load()
        {
            SensorType t = new SensorType();
            t.Name = "Humidity";
            t.typeId = 1;
            t.measurementType = MeasurementType.Percentage;
            List<SensorType> ls = new List<SensorType>();
            return ls;
        }

        public bool Remove(SensorType obj)
        {
            return true;
        }

        public async Task<bool> Save(SensorType sensorType)
        {
            if (sensorType.typeId == 0 )
            {
                return false;
            }
            return true;
        }


    }
}
