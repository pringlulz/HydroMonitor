using HydroMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;


namespace HydroMonitor.Repository
{
    public  class SensorDAO : DatabaseConnector<Sensor>
    {
        //TODO: add CRUD stuff

        public async Task<bool> Save(Sensor sensor)
        {
            await Init();
            var result = 0;
            if (sensor.SensorId != 0)
            {
                result = await _database.UpdateAsync(sensor);
                System.Diagnostics.Debug.WriteLine($"tried update");
            }
            if (result == 0)
            { //try inserting instead?
                result = await _database.InsertAsync(sensor);
                System.Diagnostics.Debug.WriteLine($"tried insert");
            }

            return result != 0;
        }

        public async Task<Sensor> Load(int sensorId)
        {
            await Init();
            return await _database.GetAsync<Sensor>(sensorId);
        }

        public async Task<List<Sensor>> Load()
        {
            await Init();
            return await _database.Table<Sensor>().ToListAsync();
        }

        public async Task<bool> Disable(Sensor sensor)
        {
            await Init();
            return await _database.ExecuteAsync("UPDATE Sensor SET enabled = false") != 0;
        }
        public async Task<bool> Remove(Sensor sensor)
        {
            await Init();
            return await _database.DeleteAsync(sensor) != 0;
        }



    }
}
