//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UppsalaApi.Models;
using UppsalaApi.Infrastructure;

namespace UppsalaApi.Controllers
{
    [Route("/[controller]")]
    public class InfoController : Controller
    {
        private readonly CampusInfo _campusInfo;

        public InfoController(IOptions<CampusInfo> campusInfoAccessor)
        {
            _campusInfo = campusInfoAccessor.Value;
            _campusInfo.Self = Link.To(nameof(GetInfo));
        }

        [HttpGet(Name = nameof(GetInfo))]
        [ResponseCache(CacheProfileName = "Static")]
        [Etag]
        public IActionResult GetInfo()
        {           
            if (!Request.GetEtagHandler().NoneMatch(_campusInfo))
            {
                return StatusCode(304, _campusInfo);
            }

            return Ok(_campusInfo);
        }
    }
}