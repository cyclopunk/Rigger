using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rigger.Attributes;

namespace aspnetcoreapp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [Autowire] private ILogger<HomeController> logger;

        [HttpGet]
        public IActionResult Index()
        {
            logger.LogInformation("Got home controller");
            dynamic obj = new ExpandoObject();
            obj.test = "hi";

            return new JsonResult(obj);
        }
    }
}
