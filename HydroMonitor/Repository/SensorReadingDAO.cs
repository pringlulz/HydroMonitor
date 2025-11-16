using HydroMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HydroMonitor.Repository
{
    public class SensorReadingDAO : DatabaseConnector<SensorReading>
    {
        public async Task<bool> Save(SensorReading reading)
        {
            await Init();
            var result = 0;
            result = await _database.InsertAsync(reading);
            return result != 0;
        }

        public async Task<List<SensorReading>> Get(int sensorId)
        {
            await Init();
            return await _database.Table<SensorReading>().Where(sr => sr.SensorId.Equals(sensorId)).ToListAsync();
        }

        //public async Task<List<SensorReading>> get

    }
}
