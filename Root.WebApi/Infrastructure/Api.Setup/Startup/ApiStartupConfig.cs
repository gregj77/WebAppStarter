using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Api.Setup.DependencyInjection;
using Autofac;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using NLog;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Api.Setup.Startup
{
    public class ApiStartupConfig
    {
        public void Configuration(IAppBuilder app)
        {
            var container = ContainerConfig.Container;

            string virtualPath = ConfigurationManager.AppSettings["virtualPathFolderName"] ?? string.Empty;

            app.Map(new PathString(""), webApiApp =>
            {
                var webApiConfig = new HttpConfiguration()
                    .ConfigureWebApi(container);
                
                webApiApp.Use(async (ctx, next) => 
                {
                    if (ctx.Request.Path.Value == "/")
                    {
                        ctx.Response.Redirect($"{virtualPath}/swagger/ui/index");
                        return;
                    }
                    await next();
                });
                
                webApiConfig
                    .EnableSwagger(c =>
                    {
                        c.RootUrl(req => SwaggerDocsConfig.DefaultRootUrlResolver(req) + virtualPath);
                        c.SingleApiVersion("v1", ConfigurationManager.AppSettings.Get("ProductName"));
                        var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"bin\Documentation.XML");
                        c.IncludeXmlComments(xmlPath);
                        c.ResolveConflictingActions(x => x.First());
                    })
                    .EnableSwaggerUi(c =>
                    {
                        c.EnableDiscoveryUrlSelector();
                        c.InjectJavaScript(typeof(ApiStartupConfig).Assembly, "Api.Setup.SwaggerExtensions.auth.js");
                    });


                webApiApp.UseCors(CorsOptions.AllowAll);
                webApiApp.UseWebApi(webApiConfig);
                
            });

            container.Resolve<ILogger>().Info("WebAPI Application started");
        }
    }
}