using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using UppsalaApi.Infrastructure;

namespace UppsalaApi.Models
{
    public class RoomResource : Resource, IEtaggable
    {
        [Sortable]
        [Searchable]
        public string Name { get; set; }

        [Sortable(Default = true)]
        [SearchableDecimal]
        public decimal Rate { get; set; }


        public Form Book { get; set; }

        public Link Openings { get; set; }

        public string GetEtag()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return Md5Hash.ForString(serialized);
        }

    }
}
