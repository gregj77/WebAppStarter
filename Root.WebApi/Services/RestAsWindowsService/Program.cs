using System;
using Topshelf;
using Topshelf.Autofac;
using Api.Setup.DependencyInjection;

namespace RestAsWindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ContainerConfig.ConfigureDependencies(new[] { typeof(Program).Assembly });

            HostFactory.Run(hc =>
            {
                hc.UseAutofacContainer(container);
                hc.Service<RestServiceStarter>(s =>
                {
                    s.ConstructUsingAutofacContainer();

                    s.WhenStarted((instance, c) =>
                    {
                        c.RequestAdditionalTime(TimeSpan.FromSeconds(60));
                        instance.OnStart();
                        return true;
                    });

                    s.WhenStopped((instance, c) =>
                    {
                        c.RequestAdditionalTime(TimeSpan.FromSeconds(60));
                        instance.OnStop();
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
