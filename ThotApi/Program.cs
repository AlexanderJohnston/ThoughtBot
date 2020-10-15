using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThotLibrary;
using Totem.App.Web;

namespace ThotApi
{
    public class Program
    {
        //static string webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        public static Task Main(string[] args)
        {
            //var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            return WebApp.Run<ThotArea>(new ConfigureWebApp().BeforeAppConfiguration(app => app.AddJsonFile("appsettings.json"))
                //.BeforeHost(host => host.UseWebRoot(webRoot))
                //.BeforeApp(app => app.ConfigureForDeveloper())
                //.BeforeServices(services => services.ConfigureServices(database))
                );
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
