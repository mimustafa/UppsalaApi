using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UppsalaApi.Models;

namespace UppsalaApi.Controllers
{
    [Route("/[controller]")]
    public class InfoController : Controller
    {
        private readonly CampusInfo _campusInfo;

        public InfoController(IOptions<CampusInfo> campusInfoAccessor)
        {
            _campusInfo = campusInfoAccessor.Value;
        }


        [HttpGet(Name = nameof(GetInfo))]
        public IActionResult GetInfo()
        {
            _campusInfo.Self = Link.To(nameof(GetInfo));

            return Ok(_campusInfo);
        }
    }
}
