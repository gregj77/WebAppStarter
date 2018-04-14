using System.Threading.Tasks;
using JobEngine.Jobs;
using NLog;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace JobEngine
{
    public class JobEngineService
    {
        private readonly ISchedulerFactory _factory;
        private readonly IJobFactory _jobFactory;
        private IScheduler _scheduler;
                   
        private readonly ILogger _logger = LogManager.GetLogger(nameof(JobEngineService));

        public JobEngineService(ISchedulerFactory factory, IJobFactory jobFactory)
        {
            _factory = factory;
            _jobFactory = jobFactory;
        }

        public async Task Start()
        {
            _scheduler = await _factory.GetScheduler();
            _scheduler.JobFactory = _jobFactory;
            await _scheduler.Start();

            var periodicJob = JobBuilder.Create<SamplePeriodicJob>().Build();
            var periodicJobSchedule = TriggerBuilder.Create().StartNow()
                .WithSimpleSchedule(s => s.WithIntervalInSeconds(30).RepeatForever()).Build();

            await _scheduler.ScheduleJob(periodicJob, periodicJobSchedule);

            _logger.Warn("service started!");
        }

        public async Task Stop()
        {
            await _scheduler.Shutdown();
            _logger.Warn("service stopped!");
        }
    }
}
