using BossFight.BossFightBackEnd.BossFightLogger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BossFight
{
    public class Startup
    {
//         public Startup(IConfiguration configuration)
//         {
//             Configuration = configuration;
//         }

//         public IConfiguration Configuration { get; }
//         private ILoggerFactory _loggerFactory;

//         public void ConfigureServices(IServiceCollection services)
//         {
//             GlobalConnection.SetConnectionString(Configuration["ConnectionStrings:DefaultConnection"]);
//             services.AddControllers();
//             services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
//             {
//                 builder.AllowAnyOrigin()
//                          .AllowAnyMethod()
//                          .AllowAnyHeader();
//             }));

//             ILoggerProvider fileLoggerProvider = new BossFightLoggerProvider("log.txt");
//             _loggerFactory = LoggerFactory.Create(builder =>
//             {
//                 builder.AddConsole();
//                 builder.AddDebug();
//                 builder.AddProvider(fileLoggerProvider);
//                 builder.SetMinimumLevel(LogLevel.Trace);
//             });
//         }

//         // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//         public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//         {
//             app.UseWebSockets();
//             app.UseCors("MyPolicy");
//             app.UseRouting();
//             app.UseAuthorization();
//             app.UseEndpoints(endpoints =>
//             {
//                 endpoints.MapControllers();
//             });

//             Console.WriteLine("Ready!");

//             var playerRegeneratorLogger = _loggerFactory.CreateLogger<PlayerRegenerator>();
//             var playerRegenerator = new PlayerRegenerator(playerRegeneratorLogger);
//             playerRegenerator.Run();

//             var playerConnectivityInformationLogger = _loggerFactory.CreateLogger<PlayerConnectivityInformation>();
//             var playerConnectivityInformation = new PlayerConnectivityInformation(playerConnectivityInformationLogger);
//             playerConnectivityInformation.Run();
//         }
    }
}
