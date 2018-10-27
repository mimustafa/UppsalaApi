using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UppsalaApi.Models;
using UppsalaApi.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UppsalaApi.Controllers
{
  
    [Route("/[controller]")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;


        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet(Name = nameof(GetRoomsAsync))]
        public async Task<IActionResult> GetRoomsAsync(CancellationToken cancellationToken)
        {
            var rooms = await _roomService.GetRoomsAsync(cancellationToken);

            var collectionLink = Link.ToCollection(nameof(GetRoomsAsync));
            var collection = new Collection<RoomResource>
            {
                Self = collectionLink,
                Value = rooms.ToArray()
            };

            return Ok(collection);
        }

        // /room/{roomId}
        [HttpGet("{roomId}", Name =nameof(GetRoomByIdAsync))]
        public async Task<IActionResult> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken)
        {

            var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
            if (room == null) return NotFound();

            return Ok(room);


        }


    }
}
