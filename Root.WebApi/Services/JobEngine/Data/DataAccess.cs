using System;
using System.Threading.Tasks;

namespace JobEngine.Data
{
    internal class DataAccess : IDataAccess
    {

        public DataAccess()
        {
        }

        public Task<long?> ReadData()
        {
            return Task.FromResult((long?)123L);
        }
    }
}
