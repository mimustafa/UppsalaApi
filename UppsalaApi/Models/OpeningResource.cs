using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UppsalaApi.Models
{
    public class OpeningResource
    {
        public Link Room { get; set; }

        public DateTimeOffset StartAt { get; set; }

        public DateTimeOffset EndAt { get; set; }

        public decimal Rate { get; set; }
    }
}
