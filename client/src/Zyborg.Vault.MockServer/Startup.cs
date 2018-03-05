using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zyborg.Vault.MockServer
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
            // As per https://www.strathweb.com/2016/12/accessing-httpcontext-outside-of-framework-components-in-asp-net-core/
            // this allows us to get at HttpContext vi DI from components that are not part
            // of the ASP.NET framework set of components proper.
            // TODO:  as per https://github.com/aspnet/Hosting/issues/793,
            // "...maintaining this state has non-trivial performance costs..."
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc(options =>
            {
                // see ApiExceptionFilter class for more details
                options.Filters.Add(new ApiExceptionFilter());
            });

            services.AddSingleton<MockServer>();
            services.AddSingleton<Func<Storage.IStorage, IConfiguration>>(
                    x => Configuration.GetSection("Zyborg.Vault.MockServer:Storage:Settings"));
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
            Configuration.Bind("Zyborg.Vault.MockServer", Server.Settings);
            Server.Start();
        }
    }
}
