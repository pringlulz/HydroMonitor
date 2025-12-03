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

        public async Task<List<SensorReading>> Load(int sensorId)
        {
            await Init();
            return await _database.Table<SensorReading>().Where(sr => sr.SensorId.Equals(sensorId)).ToListAsync();
        }

        public async Task<List<SensorReading>> Load(int sensorId, DateTime start, DateTime end)
        {
            await Init();
            return await _database.Table<SensorReading>().Where(sr => sr.SensorId.Equals(sensorId) 
                && sr.Timestamp >= start && sr.Timestamp <= end 
                && sr.Type.Equals(MeasurementType.Percentage)).ToListAsync();
        }

        public async Task<List<SensorReading>> Load(int sensorId, MeasurementType type)
        {
            await Init();
            return await _database.Table<SensorReading>().Where(sr => sr.SensorId.Equals(sensorId) && sr.Type.Equals(type) ).ToListAsync();
        }

        //public async Task<List<SensorReading>> get

    }
}
