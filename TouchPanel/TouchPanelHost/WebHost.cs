using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFSPopoutPanelManager.TouchPanel.Shared;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.TouchPanel.TouchPanelHost
{
    public class WebHost : IWebHost
    {
        private IHost _host;
        private ISimConnectService _simConnectService;

        public WebHost(IntPtr windowHandle)
        {
            _simConnectService = new SimConnectService(windowHandle);
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
                TouchPanelLogger.ServerLog($"Web Host server start error: : {ex.Message}", Shared.LogLevel.ERROR);
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
                TouchPanelLogger.ServerLog($"Web Host server stop error: : {ex.Message}", Shared.LogLevel.ERROR);
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
                TouchPanelLogger.ServerLog($"Web Host server restart error: : {ex.Message}", Shared.LogLevel.ERROR);
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
                    options.Listen(IPAddress.Any, 27010, cfg =>
                    {
                        cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
                    services.AddHostedService<WebHostService>();

                    // In production, the React files will be served from this directory
                    services.AddSpaStaticFiles(configuration =>
                    {
                        // Need to use full path here since MSFS exe.xml cannot resolve relative path
                        configuration.RootPath = Path.Combine(AppContext.BaseDirectory, @"ReactClient");
                    });

                    services.AddCors();

                    //TBD: (remove for production app for now) Allow CORS for API access, this are for REACTJS PWA APP with security cert
                    //services.AddCors(options =>
                    //{
                    //    options.AddPolicy(name: MY_ALLOW_SPECIFIC_ORIGINS,
                    //                      builder =>
                    //                      {
                    //                          builder.WithOrigins("http://localhost",
                    //                                              "http://localhost:27010",
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

                    app.UseStaticFiles();
                    app.UseSpaStaticFiles();

                    //app.UseRouting();

                    app.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = env.ContentRootPath + @"\..\..\..\..\..\TouchPanel\ReactClient";

                        if (env.IsDevelopment())
                        {
                            spa.UseReactDevelopmentServer(npmScript: "start");
                        }
                    });
                });
            })
            .UseConsoleLifetime();
    }
}