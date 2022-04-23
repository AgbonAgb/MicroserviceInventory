using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
{
    public class Startup
    {
         private const string AllowedCORSSettings = "AllowedOrigin";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMongo()
            .AddMongoRepository<InventoryItem>("inventoryitems")//pass entity and collectionNames
             .AddMongoRepository<CatalogItem>("caatalogItems")//entity
             .AddMassTransitRabbitmq(); //rabbitMQ
                                        //to avoid the same time interval for retry in API call, we introduce random
            AddCatalogClient(services);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        private static void AddCatalogClient(IServiceCollection services)
        {
            Random tt = new Random();
            //register catalog client
            services.AddHttpClient<CatLogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
            })
            //Wait and retry Policy
            //Retry geting data from Catlog incase the service having these issues Network failures, server errors,  request timeout
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))//it wil retry 5 times in 2s, 4s,8s,16s,24s
                                 + TimeSpan.FromMilliseconds(tt.Next(0, 1200)),                                                  //we can skip this
                onRetry: (outcome, timeSpan, retryAttempt) =>
                {
                    //use logger to log this
                    var serviceprovider = services.BuildServiceProvider();
                    serviceprovider.GetService<ILogger<CatLogClient>>()?
                    .LogWarning($"Delaying for {timeSpan.TotalSeconds} seconds, then making rety {retryAttempt}");
                }
            ))
            //this monitor the resource of the service provider 
            //Add CircuitBreaker Polict. 3 tries to conclude there is a problem,wait 15 seconds before allowing new request to tray again
            //calls will fail if the circuit is opened
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timeSpan) =>
                {
                    //use logger to log this
                    var serviceprovider = services.BuildServiceProvider();
                    serviceprovider.GetService<ILogger<CatLogClient>>()?
                    .LogWarning($"Opening the circuit for {timeSpan.TotalSeconds} seconds....");
                },
                onReset: () =>
                {
                    var serviceprovider = services.BuildServiceProvider();
                    serviceprovider.GetService<ILogger<CatLogClient>>()?
                    .LogWarning($"Closing the circuit....");
                }
            ))
            //timeout policy
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
            //allowable delay time when external API is called on thie https://localhost:5001, 1 seconds before giving up
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));

                //Configure Cors to take value from appsettings.Dev 
                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[AllowedCORSSettings])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
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
