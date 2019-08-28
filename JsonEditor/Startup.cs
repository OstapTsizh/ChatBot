using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StuddyBot.Core.DAL.Data;
using JsonEditor.ConfigurationModels;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JsonEditor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            services.AddTransient((s) => new StuddyBotContext(
               new DbContextOptionsBuilder<StuddyBotContext>().UseSqlServer(Configuration
                   .GetConnectionString("DefaultConnection")).Options));

            var a = GetPathSettingsFromConfig();

            services.AddSingleton<IPathSettings, PathSetting>((s)=>a);
            

            // Get settings for paths in Decision Maker
            services.Configure<PathSetting>(Configuration.GetSection("PathSettings"));
            services.Configure<PathSettingsLocal>(Configuration.GetSection("PathSettingsLocal"));



            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }


        private PathSetting GetPathSettingsFromConfig()
        {
            var _pathSetting = new PathSetting();

            AssignProperties(_pathSetting, Configuration.GetSection("PathSettings").GetChildren());           
            

            if(_pathSetting != null)
            {                
                ConfigurePaths(_pathSetting);
                return _pathSetting;
            }

            throw new Exception("Throwed in JsonEditor.Startup.GetPathSettingsFromConfig method");
        }

        /// <summary>
        /// // Configure for local or deployed project
        /// </summary>
        private void ConfigurePaths(PathSetting pathSetting)
        {
            var localPathSettings = new PathSettingsLocal();

            AssignProperties(localPathSettings, Configuration.GetSection("PathSettingsLocal").GetChildren());

            if (localPathSettings != null)
            {
                if(localPathSettings.IsLocalStart.ToLower() == "true" )
                {
                    foreach (var prop in pathSetting.GetType().GetProperties())
                    {
                        prop.SetValue(pathSetting, prop.GetValue(pathSetting)
                                                   .ToString()
                                                   .Insert(2, localPathSettings.RootForLocalStart));
                    }
                }
            }

            //throw new Exception("Throwed in JsonEditor.Startup.ConfigurePahs method");
        }


        private void AssignProperties(object settings, IEnumerable<IConfigurationSection> children)
        {            
            foreach (var prop in children)
            {
                var a = settings.GetType().GetProperty(prop.Key);
                a.SetValue(settings, prop.Value);
            }
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
