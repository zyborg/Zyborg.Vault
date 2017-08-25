using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zyborg.Vault.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Configuration.Bind("Zyborg.Vault.Server", Server.Settings);
            Server.Start();
        }

        public IConfiguration Configuration
        { get; }

        public MockServer Server
        { get; } = new MockServer();

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                // see ApiExceptionFilter class for more details
                options.Filters.Add(new ApiExceptionFilter());
            });

            services.AddSingleton(Server);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseBuffering(); // Disables "chunked" Transfer-Encoding
            app.UseMvc();
        }
    }
}
