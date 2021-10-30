using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BossFight.Models;

namespace BossFight.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RestTestController : ControllerBase
    {
        private readonly ILogger<RestTestController> _logger;
        public static List<object> MyCollection = new List<object>();

        public RestTestController(ILogger<RestTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "Get was a success";
        }

        [HttpGet("~/GetMoreStuff")]
        public string GetMoreStuff()
        {
            return "GetMoreStuff was a success";
        }

        [HttpPost]
        public string Post(RestTest postTest)
        {
            var result = "";
            if (postTest != null)
            {
                MyCollection.Add(postTest);
                result = $"added object: {postTest}";
            }
            else
                result = "ERROR PostTest item was null";

            return result;
        }
    }
}
