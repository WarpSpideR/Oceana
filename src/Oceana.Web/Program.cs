using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Oceana.Web
{
    /// <summary>
    /// Entry class for the Oceana web application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates an <see cref="IHostBuilder"/> to run the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns><see cref="IHostBuilder"/> that defines the application.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
