using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreatorsSquad.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public IEnumerable<string> Get()
        {
            return new string[] { "Hi", "MICROSOFT DOTNET CORE 3.1 WEBAPI" };
        }
    }
}