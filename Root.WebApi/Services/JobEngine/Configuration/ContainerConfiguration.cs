using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Autofac;
using Autofac.Core;
using JobEngine.Data;
using JobEngine.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace JobEngine.Configuration
{
    internal static class ContainerConfiguration
    {
        private static volatile IContainer _container;
        private static readonly object Lock = new object();

        internal static IContainer Container => _container;

        public static IContainer ConfigureDependencies()
        {
            if (null != _container) return _container;

            lock (Lock)
            {
                if (null != _container) return _container;

                var builder = new ContainerBuilder();

                builder.RegisterModule<LoggingModule>();

                string connectionString = ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                builder.Register(ctx => new SqlConnection(connectionString)).As<IDbConnection>();

                builder.RegisterType<DataAccess>().AsImplementedInterfaces();
                builder.RegisterType<JobEngineService>().AsSelf();

                var schedulingConfig = new NameValueCollection
                {
                    {"quartz.scheduler.threadName", "QuartzScheduler" },
                    {"quartz.serializer.type", "binary" }
                };
                builder.Register(ctx => (IJobFactory)new JobFactory(ctx.Resolve<IComponentContext>())).SingleInstance();
                builder.Register(ctx => (ISchedulerFactory)new StdSchedulerFactory(schedulingConfig)).SingleInstance();

                builder.RegisterType<SamplePeriodicJob>();

                var container = builder.Build();

                _container = container;


                return container;
            }
        }

        private class JobFactory : IJobFactory
        {
            private readonly IComponentContext _container;

            public JobFactory(IComponentContext container)
            {
                _container = container;
            }

            public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            {
                IJob job = _container.Resolve(bundle.JobDetail.JobType) as IJob;
                return job;
            }

            public void ReturnJob(IJob job)
            {
                (job as IDisposable)?.Dispose();
            }
        }

    }
}
