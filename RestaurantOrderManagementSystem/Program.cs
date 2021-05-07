using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace RestaurantOrderManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PreInit();
            CreateHostBuilder(args).Build().Run();
        }

        private static void PreInit()
        {
            if (!Directory.Exists("./DB"))
                Directory.CreateDirectory("./DB");
            if (!Directory.Exists("./wwwroot/Assets/Images"))
                Directory.CreateDirectory("./wwwroot/Assets/Images");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}