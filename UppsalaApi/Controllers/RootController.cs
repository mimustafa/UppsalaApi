using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UppsalaApi.Models;

namespace UppsalaApi.Controllers
{
    [Route("/")]
    [ApiVersion("1.0")]
    public class RootController : Controller
    {
        [HttpGet(Name =  nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            // TODO: Consider Refactor to RootResposne
            var resposne = new RootResource
            {
                Self = Link.To(nameof(GetRoot)),
                Rooms = Link.To(nameof(RoomsController.GetRoomsAsync)),
                Info = Link.To(nameof(InfoController.GetInfo))

            };

            return Ok(resposne);

        }
 
    }
}
