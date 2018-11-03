using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UppsalaApi.Models
{
    public class RoomsResponse : PagedCollection<RoomResource>
    {
        public Link Openings { get; set; }

        public Form RoomsQuery { get; set; }
    }
}
