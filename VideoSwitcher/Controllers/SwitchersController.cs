using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using VideoSwitcher.Core;

namespace VideoSwitcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SwitchersController : ControllerBase
    {
        private readonly Switcher switcher;

        public SwitchersController(Switcher switcher)
        {
            this.switcher = switcher;
        }

        [HttpGet]
        public IEnumerable<Switcher> Get()
        {
            yield return switcher;
        }
    }
}
