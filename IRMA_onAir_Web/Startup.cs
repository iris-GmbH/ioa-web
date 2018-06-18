using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.HttpOverrides;

using IrmaWeb.Classes.Helper;
using static IrmaWeb.Models.RuntimeSettings;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IrmaWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddOptions();

            // Init. default config values. (also fallback when missing in config)
            YamiStatic = new Yami
            {
                Settings = new Settings
                {
                    Debug = false,
                    Scheme = "https",
                    Host = "api.irmaonair.com",
                    Path = "services/REST/apc",
                    Version = "v1",
                    Release = "r5",
                    ZipCompressionLevel = "Fastest",
                    RESTCompressionLevel = "GZip",
                    TimeoutREST = 5,
                    DayLimit = 31,
                    SessionTimeout = 30240,
                    SessionCache = false
                },
                SettingsEx = new SettingsEx
                {
                    User = "user/devices",
                    Stops = "stops",
                    Reports = "reports",
                    Tracks = "tracks"
                }
            };

            // Override fallbacks with real config values when there.
            YamiStatic = Configuration.GetSection("Yami").Get<Yami>();
            services.Configure<Yami>(Configuration.GetSection("Yami"));

            // Configure Authentication settings
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = new TimeSpan(0, YamiStatic.Settings.SessionTimeout, 0);
                options.LoginPath = "/";
                options.LogoutPath = "/Home/Logout";
            });

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddFile(Configuration.GetSection("Logging"));
            LogHelper.LoggerFactory = loggerFactory; //Give over LoggerFactory to static loghelper

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");
            
            app.UseMiddleware<HttpMiddleware>();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
