using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using NLog;
using Owin;
using Swashbuckle.Application;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace RestAsWindowsService
{
    public class RestServiceStarter
    {
        // this should be read from configuration. port number can be tracked using semaphore
        private string _url = "http://localhost:42001";
        private HttpConfiguration _configuration;
        private IDisposable _context;
        private ILogger _logger;

        public RestServiceStarter(HttpConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnStart()
        {
            _context = WebApp.Start(_url, Configure);
        }

        public void OnStop()
        {
            _logger.Info("stopping...");
            _context.Dispose();
        }

        private void Configure(IAppBuilder app)
        {
            _logger.Info("starting web app");
            string virtualPath = "";

            app.Map(new PathString(""), webApiApp =>
            {
                _logger.Info($"configuring webApi on {_url}...");
                webApiApp.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path.Value == "/")
                    {
                        ctx.Response.Redirect($"{virtualPath}/swagger/ui/index");
                        return;
                    }
                    await next();
                });

                // consider if swagger is required for this setup...
                _configuration
                    .EnableSwagger(c =>
                    {
                        c.RootUrl(req => SwaggerDocsConfig.DefaultRootUrlResolver(req) + virtualPath);
                        c.SingleApiVersion("v1", ConfigurationManager.AppSettings.Get("ProductName"));
                        // Requires setting up persmission for the file access
                        //var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Documentation.XML");
                        //c.IncludeXmlComments(xmlPath);
                        c.ResolveConflictingActions(x => x.First());
                    })
                    .EnableSwaggerUi(c =>
                    {
                        c.EnableDiscoveryUrlSelector();
                    });


                webApiApp.UseWebApi(_configuration);

            });
            _logger.Info("running");
        }
    }
}
