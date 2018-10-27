using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace UppsalaApi.Models
{
    public class RoomResource : Resource
    {
        public string Name { get; set; }
        public decimal Rate { get; set; }
    }
}
