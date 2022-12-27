using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using ThotLibrary;
using Totem.App.Service;

namespace ThotService
{
    class Program
    {
        public static Task Main(string[] args)
        {
            var configuration = new ConfigureServiceApp().BeforeAppConfiguration(app => app.AddJsonFile("appsettings.json"));
            return ServiceApp.Run<ThotArea>(configuration/*.BeforeServices(providers => providers.AddSingleton<IElasticService, ElasticService>())*/);
        }
    }

}
