using HydroMonitor.Interfaces;
using HydroMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HydroMonitor.Repository;

namespace HydroMonitorTests.MockRepositories
{
    public class MockSensorDAO : SensorDAO
    {
        private Random rnd = new Random();

        public MockSensorDAO(IRepository<SensorType> stDAO) : base(stDAO)
        {
        }

        public Task<Sensor> Load(int pk)
        {
            Sensor fake = new Sensor();
            fake.SensorId = pk;
            fake.macAddress = "ab:cd:de:f0:01:02";
            return Task.FromResult(fake);
        }

        public Task<List<Sensor>> Load()
        {
            
            List<Sensor> list = new List<Sensor>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(Load(rnd.Next(1, 150) ).Result);
            }
            return Task.FromResult(list);
        }

        public Task<bool> Remove(Sensor obj)
        { //set to return a failure 10% of the time... we should still 
            if (obj.SensorId != 0)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
            
        }

        public Task<bool> Save(Sensor obj)
        {   //since this is a test, maybe we check the obejct?
            if (obj.SensorId != 0)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}

