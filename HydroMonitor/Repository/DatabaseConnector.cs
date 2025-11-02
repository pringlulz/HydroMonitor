using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace HydroMonitor.Repository
{
    public class DatabaseConnector<T> where T : class, new()
    {

        protected SQLiteAsyncConnection? _database;

        protected async Task Init()
        {
            if (_database is not null)
            {
                return;
            } 
            //this probably opens a new connection for each type of object. There a way we can get this to use connection pooling?
            _database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "HydroMon.db3"), SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.Create);
            //_ = await _database.ExecuteAsync("CREATE TABLE sensors(id  name var")
            System.Diagnostics.Debug.WriteLine($"Init'd database");
            _ = await _database.CreateTableAsync<T>();
            System.Diagnostics.Debug.WriteLine($"Init'd table");


        }

        protected async Task Destroy()
        {
            if (_database != null)
                await _database.CloseAsync();
        }

    }
}
