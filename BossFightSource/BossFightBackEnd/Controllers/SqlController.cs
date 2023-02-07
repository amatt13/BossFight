using Microsoft.AspNetCore.Mvc;
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