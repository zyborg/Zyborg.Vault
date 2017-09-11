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
        }

        public IConfiguration Configuration
        { get; }

        public MockServer Server
        { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                // see ApiExceptionFilter class for more details
                options.Filters.Add(new ApiExceptionFilter());
            });

            services.AddSingleton<MockServer>();
            services.AddSingleton<Func<Storage.IStorage, IConfiguration>>(
                    x => Configuration.GetSection("Zyborg.Vault.Server:Storage:Settings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, MockServer server)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<RequestMiddleware>();

            app.UseResponseBuffering(); // Disables "chunked" Transfer-Encoding
            app.UseMvc();

            Server = server;
            Configuration.Bind("Zyborg.Vault.Server", Server.Settings);
            Server.Start();
        }
    }
}
