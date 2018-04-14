using System.Threading.Tasks;

namespace JobEngine.Data
{
    internal interface IDataAccess
    {
        Task<long?> ReadData();
    }
}