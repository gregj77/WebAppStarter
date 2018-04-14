using System.Threading.Tasks;
using JobEngine.Data;
using NLog;
using Quartz;

namespace JobEngine.Jobs
{
    internal class SamplePeriodicJob : IJob
    {

        private readonly IDataAccess _dataAccess;
        private readonly ILogger _logger;

        public SamplePeriodicJob(IDataAccess dataAccess, ILogger logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            long? task = await _dataAccess.ReadData();
            if (task.HasValue)
            {
                _logger.Debug($"task result {task.Value}");
            }
            else
            {
                _logger.Debug("no data...");
            }
        }
    }
}
