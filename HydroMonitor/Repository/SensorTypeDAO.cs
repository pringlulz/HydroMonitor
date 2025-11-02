using HydroMonitor.Interfaces;
using HydroMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitor.Repository
{
    public class SensorTypeDAO : DatabaseConnector<SensorType>, IRepository<SensorType>
    {

        public SensorTypeDAO()
        {
            AddDefaultSensorTypes();
        }


        public async void AddDefaultSensorTypes()
        {
            await Init();
            SensorType check = Load("Humidity").Result;
            if (check.typeId == 0) { 
                SensorType newSensorType = new SensorType
                {
                    Name = "Humidity",
                    measurementType = MeasurementType.Percentage
                };
                await _database.InsertAsync(newSensorType);
            }
            //TODO: add the rest of the sensor types

        }

        public async Task<SensorType> Load(String name)
        { //load by name instead of typeId
            await Init();
            return await _database.Table<SensorType>().Where(st => st.Name.Equals(name)).FirstOrDefaultAsync();
        }


        public async Task<SensorType> Load(int pk)
        {
            await Init();
            return await _database.GetAsync<SensorType>(pk);
        }

        public async Task<List<SensorType>> Load()
        {
            await Init();
            return await _database.Table<SensorType>().ToListAsync();
        }

        public async Task<bool> Remove(SensorType obj)
        {
            await Init();
            return await _database.DeleteAsync(obj) != 0;
        }

        public async Task<bool> Save(SensorType sensorType)
        {
            await Init();
            var result = 0;
            if (sensorType.typeId != 0)
            {
                result = await _database.UpdateAsync(sensorType);
                System.Diagnostics.Debug.WriteLine($"tried update");
            }
            if (result == 0)
            { //try inserting instead?
                result = await _database.InsertAsync(sensorType);
                System.Diagnostics.Debug.WriteLine($"tried insert");
            }
            return result != 0;
        }

    }
}
