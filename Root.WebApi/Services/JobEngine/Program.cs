using System;
using Autofac;
using JobEngine.Configuration;
using Quartz;
using Topshelf;
using Topshelf.Autofac;

namespace JobEngine
{
    public class Program
    {
        static void Main(string[] args)
        {
            var container = ContainerConfiguration.ConfigureDependencies();

            HostFactory.Run(hc =>
            {
                hc.UseAutofacContainer(container);
                hc.Service<JobEngineService>(s =>
                {
                    s.ConstructUsingAutofacContainer();

                    s.WhenStarted((instance, c) =>
                    {
                        c.RequestAdditionalTime(TimeSpan.FromSeconds(60));
                        instance.Start().Wait();
                        return true;
                    });

                    s.WhenStopped((instance, c) =>
                    {
                        c.RequestAdditionalTime(TimeSpan.FromSeconds(60));
                        instance.Stop().Wait();
                        container.Dispose();
                        return true;
                    });
                });

                hc.RunAsLocalSystem();
                hc.StartAutomatically();
                hc.UseNLog();

                hc.EnableServiceRecovery(src =>
                {
                    src.RestartService(1);
                    src.RestartService(1);
                    src.RestartService(1);
                    src.SetResetPeriod(1);
                });
            });
        }
    }
}
