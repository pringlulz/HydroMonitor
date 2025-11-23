using HydroMonitor.Interfaces;
using HydroMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitor.Repository
{
    internal class SensorLocationDAO : DatabaseConnector<SensorLocation>, IRepository<SensorLocation>
    {
        public async Task<SensorLocation> Load(int pk)
        {
            await Init();
            try
            {
                return await _database.GetAsync<SensorLocation>(pk);
             } catch (InvalidOperationException) {
                return new SensorLocation();
            }
}

        public async Task<List<SensorLocation>> Load()
        {
            await Init();
            try
            {
                return await _database.Table<SensorLocation>().ToListAsync();
            } catch (InvalidOperationException)
            {
                return new List<SensorLocation>();
            }
        }

        public Task<bool> Remove(SensorLocation obj)
        {
            
            throw new NotImplementedException();
        }

        public async Task<bool> Save(SensorLocation obj)
        {
            await Init();
            if (obj.SensorId == 0)
            { //can't save a location for a sensor that doesn't exist
                return false;
            }

            var result = 0;
            result = await _database.UpdateAsync(obj);
            System.Diagnostics.Debug.WriteLine($"tried update");
            if (result == 0)
            { //try inserting instead?
                result = await _database.InsertAsync(obj);
                System.Diagnostics.Debug.WriteLine($"tried insert");
            }



            return true;
        }
    }
}
