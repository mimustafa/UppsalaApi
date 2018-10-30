using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UppsalaApi.Infrastructure;

namespace UppsalaApi.Models
{
    public class RoomResource : Resource
    {
        [Sortable]
        public string Name { get; set; }

        [Sortable(Default = true)]
        public decimal Rate { get; set; }
    }
}
