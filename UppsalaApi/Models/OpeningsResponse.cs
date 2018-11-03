using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UppsalaApi.Models
{
    public class OpeningsResponse : PagedCollection<OpeningResource>
    {
        public Form OpeningsQuery { get; set; }
    }
}
