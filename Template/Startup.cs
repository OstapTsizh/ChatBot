// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using DecisionMakers;
using LoggerService;
using Services;
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
using Microsoft.Bot.Schema;
using StuddyBot.Core.Models;

namespace StuddyBot
{
    public class Startup
    {
        public Startup()
        {
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            services.AddTransient((s) => new DialogInfo());

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // LOGGER
            services.AddSingleton((s) => new ThreadedLogger(s.GetService<IUnitOfWork>()));

            // Create the Decision Maker which looks for proper answers/next questions
            services.AddSingleton<IDecisionMaker, DecisionMaker>();

            services.AddTransient((s) => new StuddyBotContext(
                new DbContextOptionsBuilder<StuddyBotContext>().UseSqlServer(
                    @"Server=(localdb)\mssqllocaldb;Database=StuddyBotDB;Integrated Security=True;").Options));

              services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            //services.AddSingleton<NotificationService>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
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
