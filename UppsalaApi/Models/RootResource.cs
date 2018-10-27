using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace UppsalaApi.Models
{
    public class RootResource : Resource
    {
        public Link Info { get; set; }
        public Link Rooms { get; set; }
    }
}
