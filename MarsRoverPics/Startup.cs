using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using MarsRoverPics.Proxies;
using MarsRoverPics.Proxies.Contracts;
using MarsRoverPics.Services;
using MarsRoverPics.Services.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarsRoverPics
{
    public class Startup
    {
        //constants to hold the name of the sections in the configuration file
        private const string PICTURE_SERVICE_SECTION = "pictureService";
        private const string NASDA_API_SECION = "nasaApi";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           //configure the settings from the appsettings.json file
            services.Configure<PictureServiceSettings>(Configuration.GetSection(PICTURE_SERVICE_SECTION));
            services.Configure<NasaApiSettings>(Configuration.GetSection(NASDA_API_SECION));
            //inject proxy and service class
            services.AddScoped<INasaApiProxy, NasaApiProxy>();
            services.AddScoped<IPictureService, PictureService>();
            //inject flurl's http client factory
            services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
            //inject file system helper 
            services.AddScoped<IFileSystem, FileSystem>();
            //add memory cache (we should use distributed if we run  multiple instances of this container/app)
            services.AddMemoryCache();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
