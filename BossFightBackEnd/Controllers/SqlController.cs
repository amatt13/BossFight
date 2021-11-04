using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BossFight.Models;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using BossFight.Models.DB;

namespace BossFight.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SqlController : ControllerBase
    {
        public AppDb Db { get; }

        public SqlController(AppDb db)
        {
            Db = db;
        }
    }
}