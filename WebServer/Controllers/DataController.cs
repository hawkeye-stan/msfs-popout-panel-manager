using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.SimConnectAgent.TouchPanel;
using MSFSPopoutPanelManager.TouchPanelAgent;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace MSFSPopoutPanelManager.WebServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : BaseController
    {
        private ISimConnectService _simConnectService;
        private IMemoryCache _memoryCache;
        private IWebHostEnvironment _hostingEnvironment;
        private TouchPanelConfigSetting _touchPanelConfigSetting;

        public DataController(IConfiguration configuration, IMemoryCache memoryCache, ISimConnectService simConnectService, IWebHostEnvironment environment) : base(configuration, memoryCache, simConnectService)
        {
            _simConnectService = simConnectService;
            _memoryCache = memoryCache;
            _hostingEnvironment = environment;

            _touchPanelConfigSetting = _simConnectService.TouchPanelConfigSetting;
        }

        [HttpGet("/getdata")]
        public SimConnectData GetData()
        {
            try
            {
                var data = _memoryCache.Get<string>("simdata");
                var arudinoStatus = _memoryCache.Get<bool>("arduinoStatus");
                var msfsStatus = _memoryCache.Get<bool>("msfsStatus");
                var simSystemEvent = _memoryCache.Get<string>("simSystemEvent");

                return new SimConnectData { Data = data, MsfsStatus = Convert.ToBoolean(msfsStatus), ArduinoStatus = Convert.ToBoolean(arudinoStatus), SystemEvent = simSystemEvent };
            }
            catch
            {
                return new SimConnectData { Data = null, MsfsStatus = false, ArduinoStatus = false, SystemEvent = null, G1000NxiFlightPlan = null };
            }
        }

        [HttpPost("/postdata")]
        public IActionResult PostData(SimConnectActionData actionData)
        {
            _simConnectService.ExecAction(actionData);

            //var clientIP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            //FileLogger.WriteLog($"ClientIP: {clientIP,-20} Action: {actionData.Action,-35} Value: {actionData.ActionValue,-7}", StatusMessageType.Debug);

            return Ok();
        }

        [HttpPost("/posttouchpanelloaded")]
        public IActionResult PostTouchPanelLoaded(TouchPanelLoadedPostData data)
        {
            _simConnectService.ResetSimConnectDataArea(data.PlaneId);
            return Ok();
        }

        [HttpGet("/getplanepanelprofileinfo")]
        public string GetPlanePanelProfileInfo()
        {
            try
            {
                var planeProfileConfiguration = TouchPanelManager.GetPlaneProfiles();

                return JsonConvert.SerializeObject(planeProfileConfiguration, Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }
            catch (Exception ex)
            {
                FileLogger.WriteException(ex.Message, ex);
            }

            return string.Empty;
        }

        [HttpGet("/gettouchpanelsettings")]
        public TouchPanelConfigSetting GetTouchPanelConfig()
        {
            try
            {
                return _touchPanelConfigSetting;
            }
            catch (Exception ex)
            {
                FileLogger.WriteException(ex.Message, ex);
            }

            return new TouchPanelConfigSetting();
        }
    }

    public class SimConnectData
    {
        public string Data { get; set; }

        public bool MsfsStatus { get; set; }

        public bool ArduinoStatus { get; set; }

        public string SystemEvent { get; set; }

        public string G1000NxiFlightPlan { get; set; }
    }

    public class TouchPanelLoadedPostData
    {
        public string PlaneId { get; set; }
    }
}
