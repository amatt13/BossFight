using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostTestController : ControllerBase
    {
        private readonly ILogger<PostTestController> _logger;
        public static List<object> MyCollection = new List<object>();

        public PostTestController(ILogger<PostTestController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public string Post(PostTest postTest)
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
