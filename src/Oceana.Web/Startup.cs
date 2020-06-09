using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Oceana.Web
{
    /// <summary>
    /// Provides startup configuration and initialization.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration options.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the global configuration values.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures services required by the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to
        /// initialize.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        /// <summary>
        /// Configures the application request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
        /// <param name="env">Environment contextual information.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
