using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFSPopoutPanelManager.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSPopoutPanelManager.WebServer
{
    public class WebHost : IWebHost, ITouchPanelServer
    {
        private IHost _webHost;
        private IHost _apiHost;
        private ISimConnectService _simConnectService;
        private IntPtr _windowHandle;
        private string _currentDataList;

        private int _dataRefreshInterval = 200;
        private int _mapRefreshInterval = 250;
        private bool _isUsedArduino = false;
        private bool _isEnabledSound = true;

        public WebHost()
        {

            ServerStarted = false;
        }

        public bool ServerStarted { get; set; }

        public IntPtr WindowHandle { set { _windowHandle = value; } }

        public int DataRefreshInterval { set { _dataRefreshInterval = value; } }

        public int MapRefreshInterval { set { _mapRefreshInterval = value; } }

        public bool IsUsedArduino { set { _isUsedArduino = value; } }

        public bool IsEnabledSound { set { _isEnabledSound = value; } }

        public IServiceProvider Services
        {
            get { return _apiHost.Services; }
        }

        public void Dispose()
        {
            _apiHost.Dispose();
            _webHost.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (_simConnectService == null)
                    _simConnectService = new SimConnectService(_windowHandle);

                _simConnectService.TouchPanelConfigSetting = new TouchPanelConfigSetting()
                {
                    DataRefreshInterval = _dataRefreshInterval,
                    MapRefreshInterval = _mapRefreshInterval,
                    IsEnabledSound = _isEnabledSound,
                    IsUsedArduino = _isUsedArduino
                };

                _apiHost = CreatApiHostBuilder().Build();
                _simConnectService.SetMemoryCache(_apiHost.Services.GetService<IMemoryCache>());

                _webHost = CreateWebHostBuilder().Build();

                await _apiHost.StartAsync();
                await _webHost.StartAsync();

                ServerStarted = true;
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"Web Host server start error: {ex.Message}", ex);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _apiHost.StopAsync();
                await _webHost.StopAsync();
                _apiHost.Dispose();
                _webHost.Dispose();

                ServerStarted = false;
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"API Host server stop error: {ex.Message}", ex);
            }
        }

        public async Task RestartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await StopAsync();
                Thread.Sleep(2000);
                await StartAsync();
            }
            catch (Exception ex)
            {
                FileLogger.WriteException($"API Host server restart error: {ex.Message}", ex);
            }
        }

        private IHostBuilder CreateWebHostBuilder() =>
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
                   options.Listen(IPAddress.Any, Constants.WEB_HOST_PORT, cfg =>
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
               })
               .Configure((app) =>
               {
                   var env = app.ApplicationServices.GetService<IWebHostEnvironment>();

                   app.UseExceptionHandler("/Error");

                   app.UseCors(builder => builder
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials()
                   );

                   app.UseStaticFiles();
                   app.UseSpaStaticFiles();

                   app.UseSpa(spa =>
                   {
                       spa.Options.SourcePath = env.ContentRootPath + @"\..\..\..\..\..\..\ReactClient";

                       if (env.IsDevelopment())
                       {
                           spa.UseReactDevelopmentServer(npmScript: "start");
                       }
                   });
               });
           })
           .UseConsoleLifetime();

        private IHostBuilder CreatApiHostBuilder() =>
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
                    options.Listen(IPAddress.Any, Constants.API_HOST_PORT, cfg =>
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
                        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    });
                    services.AddMemoryCache();
                    services.AddHostedService<WebApiHostService>();
                    services.AddSingleton<ISimConnectService>(provider => _simConnectService);

                    services.AddCors();
                })
                .Configure((app) =>
                {
                    var env = app.ApplicationServices.GetService<IWebHostEnvironment>();

                    app.UseExceptionHandler("/Error");

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

                    //app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(15) });

                    //Setup websocket server
                    //app.Map("/ws", builder =>
                    //{
                    //    builder.Use(async (context, next) =>
                    //    {
                    //        try
                    //        {
                    //            if (context.WebSockets.IsWebSocketRequest)
                    //            {
                    //                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    //                //await webSocket.SendAsync(Encoding.UTF8.GetBytes("WebSocketConnected"), WebSocketMessageType.Text, true, CancellationToken.None);

                    //                Task.Run(() => SendMessage(webSocket));
                    //                await ReceiveMessage(webSocket);

                    //                return;
                    //            }
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            Debug.WriteLine(e.Message);
                    //        }
                    //        finally
                    //        {
                    //            if (!context.Response.HasStarted)
                    //                await next();
                    //        }
                    //    });
                    //});
                });
            })
            .UseConsoleLifetime();

        #region WebSocket 

        private async Task SendMessage(WebSocket webSocket)
        {
            int state = 0;

            while (webSocket != null && (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent))
            {
                List<WebSocketSendData> dataList = new List<WebSocketSendData>();
                dataList.Add(new WebSocketSendData() { panelName = "Panel1", xPos = 900, yPos = 1000, width = 850, height = 800, alwaysOnTop = state % 20 != 0, hideTitleBar = false, fullScreenMode = false, touchEnabled = false });
                dataList.Add(new WebSocketSendData() { panelName = "Panel2", xPos = 900, yPos = 1000, width = 850, height = 800, alwaysOnTop = true, hideTitleBar = true, fullScreenMode = false, touchEnabled = false });
                dataList.Add(new WebSocketSendData() { panelName = "Panel3", xPos = 900, yPos = 1000, width = 850, height = 800, alwaysOnTop = true, hideTitleBar = true, fullScreenMode = false, touchEnabled = false });

                var data = JsonConvert.SerializeObject(dataList);

                if (data != _currentDataList)
                {
                    try
                    {
                        await webSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception e)
                    {
                        webSocket = null;
                        Debug.WriteLine(e.Message);
                    }
                }

                await Task.Delay(250);
                state++;

                _currentDataList = data;
            }
        }

        private async Task ReceiveMessage(WebSocket webSocket)
        {

            byte[] buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                ParseAndExecuteMessage(buffer, result, webSocket);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private void ParseAndExecuteMessage(byte[] buffer, WebSocketReceiveResult result, WebSocket webSocket)
        {
            var messageData = Encoding.UTF8.GetString(buffer, 0, result.Count);
            if (!String.IsNullOrEmpty(messageData))
                Debug.WriteLine("Message received: " + messageData);

            if (messageData != null)
            {
                switch (messageData)
                {
                    case "RequestPanelData":
                        webSocket.SendAsync(Encoding.UTF8.GetBytes(_currentDataList), WebSocketMessageType.Text, true, CancellationToken.None);
                        break;
                }
            }
        }

        #endregion
    }
}
