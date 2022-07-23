using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace MSFSPopoutPanelManager.WebServer.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        public IConfiguration Configuration { get; }

        protected IMemoryCache MemoryCache;

        protected ISimConnectService SimConnectService;

        public BaseController(IConfiguration configuration, IMemoryCache memoryCache, ISimConnectService simConnectService)
        {
            Configuration = configuration;
            MemoryCache = memoryCache;
            SimConnectService = simConnectService;
        }
    }
}
