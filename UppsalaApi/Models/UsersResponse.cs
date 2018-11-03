using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UppsalaApi.Models
{
    public class UsersResponse : PagedCollection<UserResource>
    {
        public Form Register { get; set; }

        public Link Me { get; set; }
    }
}
