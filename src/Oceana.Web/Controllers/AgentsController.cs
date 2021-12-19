using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Oceana.Web.Controllers
{
    /// <summary>
    /// Provides a means to manage agents within the system.
    /// </summary>
    [Route("Agents")]
    public class AgentsController : Controller
    {
        /// <summary>
        /// Provides a list of available agents.
        /// </summary>
        /// <returns>Result of the action.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
