using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace UppsalaApi.Controllers
{
    [Route("/")]
    [ApiVersion("1.0")]
    public class RootController : Controller
    {
        [HttpGet(Name =  nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            var resposne = new
            {
                href = Url.Link(nameof(GetRoot), null),
                rooms = new {href = Url.Link(nameof(RoomsController.GetRooms), null)}
            };

            return Ok(resposne);

        }
 
    }
}
