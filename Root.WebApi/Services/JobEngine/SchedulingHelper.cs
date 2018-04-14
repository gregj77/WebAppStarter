using JobEngine.Configuration;
using JobEngine.Jobs;
using Quartz;

namespace JobEngine
{
    internal static class SchedulingHelper
    {
        /*
        public static void ConfigureSampleJob(this QuartzConfigurator configurator, JobEngineConfiguration config)
        {
            configurator
                .WithJob(CreateDownloadDailyReconFileJob)
                .ConfigureJobSchedule(config.SampleJobSchedule);
        }

        private static IJobDetail CreateDownloadDailyReconFileJob()
        {
            return JobBuilder.Create<SamplePeriodicJob>().Build();
        }

        private static void ConfigureJobSchedule(this QuartzConfigurator configurator, string schedule)
        {
            configurator.AddTrigger(() => TriggerBuilder.Create().WithCronSchedule(schedule).Build());
        }
        */
    }
}
