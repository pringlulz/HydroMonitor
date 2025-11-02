using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitor.Interfaces
{
    internal interface IRepository<T>
    { //Represents the basic CRUD operations of a repository.
        public Task<bool> Save(T obj);
        public Task<T> Load(int pk);
        public Task<List<T>> Load();
        public Task<bool> Remove(T obj);
    }
}
