using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RestService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace RestService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AwakeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public AwakeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            return new JsonResult("Ok");
        }
        



    }
}
