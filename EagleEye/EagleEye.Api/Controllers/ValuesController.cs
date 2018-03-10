using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EagleEye.Api.Controllers
{
    [Route("[controller]")]
    public class ValuesController : Controller
    {
        private AppConfiguration _configuration;

        public ValuesController(AppConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET api/values
        //[HttpGet]
        //public string Get()
        //{
        //    return _configuration.ConnectionString;
        //}
    }
}
