using HydroMonitor.Interfaces;
using HydroMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;


namespace HydroMonitor.Repository
{
    public  class SensorDAO : DatabaseConnector<Sensor>, IRepository<Sensor>, IDisposable
    {
        private bool disposedValue;
        private readonly IRepository<SensorType> _stDAO;

        //TODO: add CRUD stuff
        public SensorDAO(IRepository<SensorType> stDAO)
        {
            _stDAO = stDAO;
        }


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

            try
            {
                Sensor result = await _database.GetAsync<Sensor>(sensorId);
                if (result.SensorTypeId != 0)
                {
                    result.SensorType = await _stDAO.Load(result.SensorTypeId);
                }
                return result;
            } catch (InvalidOperationException) {
                return new Sensor();
            }
            
        }

        //public async Task<Sensor>

        public async Task<List<Sensor>> Load()
        {
            await Init();
            
            return await _database.Table<Sensor>().ToListAsync();
        }

        public async Task<bool> Disable(Sensor sensor)
        {
            await Init();
            return await _database.ExecuteAsync("UPDATE Sensor SET enabled = false WHERE sensorId = ?", sensor.SensorId)!= 0;
        }
        public async Task<bool> Remove(Sensor sensor)
        {
            await Init();
            return await _database.DeleteAsync(sensor) != 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //think this is correct?
                    _database.CloseAsync();
                    _database = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SensorDAO()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
