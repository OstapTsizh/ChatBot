// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using DecisionMakers;
using LoggerService;
using Services;

using Autofac;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Builder.Dialogs.Internals;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StuddyBot.Bots;
using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.BLL.Repositories;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.Interfaces;
using StuddyBot.Dialogs;
using System.Linq;
using System.Collections.Concurrent;
using System.Configuration;
using EmailSender;
using EmailSender.Interfaces;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StuddyBot.Core.Models;
using Services.Helpers;
using Services.Helpers.Interfaces;
using StuddyBot.Core.BLL.Helpers;

namespace StuddyBot
{
    public class Startup
    {
        public IConfiguration Configuration;

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            var storageConnectionString = Configuration.GetSection("ConnectionStrings")["StorageConnectionString"];
            var storageContainerName = Configuration.GetSection("ConnectionStrings")["StorageContainerName"];


            // Get settings for paths in Decision Maker
            services.Configure<PathSettings>(Configuration.GetSection("PathSettings"));
            services.Configure<PathSettingsLocal>(Configuration.GetSection("PathSettingsLocal"));

            var pathSettingsLocal = Configuration.GetSection("PathSettingsLocal")["IsLocalStart"];

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            if (pathSettingsLocal == "true")
            {
                services.AddSingleton<IStorage, MemoryStorage>();
            }
            else
            {
                //services.AddSingleton<IStorage, BotAzureBlobStorage>(
                //    (s) => new BotAzureBlobStorage(storageConnectionString, storageContainerName));

                services.AddSingleton<IStorage, AzureBlobStorage>(
                    (s) => new AzureBlobStorage(storageConnectionString, storageContainerName));
            }



            

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            services.AddSingleton<IStatePropertyAccessor<DialogInfo>>(s=>s.GetRequiredService<UserState>().CreateProperty<DialogInfo>(nameof(DialogInfo)));


            // Create the model with information about a Dialog.
            //services.AddTransient((s) => new DialogInfo()); //AddTransient((s) => new DialogInfo());

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // LOGGER
            services.AddSingleton((s) => new ThreadedLogger(s.GetService<IUnitOfWork>()));

            // Create the Decision Maker which looks for proper answers/next questions
            services.AddSingleton<IDecisionMaker, DecisionMaker>();

            //Create the Subscription Manager for user subscriptions.
            services.AddSingleton<ISubscriptionManager, SubscriptionManager>();

            // Get settings for EmailSender.
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));

            

            // Create the Email Sender for sending emails for users.
            services.AddSingleton<IEmailSender, EmailSender.EmailSender>(); //AddSingleton<IEmailSender, EmailSender.EmailSender>();

            var defaultConnectionString = Configuration.GetConnectionString("DefaultConnection");
            // Create the database context as StuddyBotContext.
            services.AddDbContext<StuddyBotContext>(options => 
            options.UseSqlServer(defaultConnectionString), ServiceLifetime.Transient);
            //services.AddTransient((s) => new StuddyBotContext(
            //    new DbContextOptionsBuilder<StuddyBotContext>().UseSqlServer(defaultConnectionString).Options));//,
            //    //defaultConnectionString));

            // Create a pattern Unit Of Work for accessing Database.
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            // Create a dictionary with Conversation References to make possible user notification for courses.
            //services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            services.AddScoped<IGetCoursesHelper, GetCoursesHelper>();

            //services.AddSingleton<NotificationService>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>(); //AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>(); // AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
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
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }


    }
}
