using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.TouchPanel.TouchPanelHost
{
    public class WebApiHost : IWebApiHost
    {
        private IHost _host;
        private ISimConnectService _simConnectService;
        private TouchPanelConfigSetting _touchPanelConfigSetting;

        public WebApiHost(IntPtr windowHandle)
        {
            _simConnectService = new SimConnectService(windowHandle);
        }

        public TouchPanelConfigSetting TouchPanelConfigSetting
        {
            get
            {
                return _touchPanelConfigSetting;
            }
            set
            {
                _touchPanelConfigSetting = value;
                _simConnectService.TouchPanelConfigSetting = value;
            }
        }

        public IServiceProvider Services
        {
            get { return _host.Services; }
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                _host = CreateHostBuilder().Build();
                _simConnectService.SetMemoryCache(_host.Services.GetService<IMemoryCache>());
                await _host.StartAsync();
            }
            catch (Exception ex)
            {
                TouchPanelLogger.ServerLog($"API Host server start error: : {ex.Message}", Shared.LogLevel.ERROR);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _host.StopAsync();
            }
            catch (Exception ex)
            {
                TouchPanelLogger.ServerLog($"API Host server stop error: : {ex.Message}", Shared.LogLevel.ERROR);
            }
        }

        public async Task RestartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await StopAsync();
                _host.Dispose();
                Thread.Sleep(2000);
                await StartAsync();
            }
            catch (Exception ex)
            {
                TouchPanelLogger.ServerLog($"API Host server restart error: : {ex.Message}", Shared.LogLevel.ERROR);
            }
        }
        private IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .UseKestrel()
                .SuppressStatusMessages(true)
                .ConfigureKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 27011, cfg =>
                    {
                        cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
                    services.AddControllersWithViews().AddJsonOptions(opts =>
                    {
                        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        opts.JsonSerializerOptions.IgnoreNullValues = true;
                    });
                    services.AddMemoryCache();
                    services.AddHostedService<WebApiHostService>();
                    services.AddSingleton<ISimConnectService>(provider => _simConnectService);

                    services.AddCors();
                    // TBD: (remove for production app for now) Allow CORS for API access, this are for REACTJS PWA APP with security cert
                    //services.AddCors(options =>
                    //{
                    //    options.AddPolicy(name: MY_ALLOW_SPECIFIC_ORIGINS,
                    //                      builder =>
                    //                      {
                    //                          builder.WithOrigins("http://localhost:27011",
                    //                                              "https://msfstouchpanel.ddns.net/",
                    //                                              "http://msfstouchpanel.ddns.net/").AllowAnyMethod().AllowAnyHeader();
                    //                      });
                    //});
                })
                .Configure((app) =>
                {
                    var env = app.ApplicationServices.GetService<IWebHostEnvironment>();

                    app.UseExceptionHandler("/Error");
                    //app.UseHsts();      // use HTTPS to allow client side webservice worker  
                    //app.UseCors(MY_ALLOW_SPECIFIC_ORIGINS);

                    app.UseCors(builder => builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed((host) => true)
                        .AllowCredentials()
                    );

                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller}/{action=Index}/{id?}");
                    });
                });
            })
            .UseConsoleLifetime();
    }
}