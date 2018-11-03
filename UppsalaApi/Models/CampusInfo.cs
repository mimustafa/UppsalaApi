﻿using System;
using Newtonsoft.Json;
using UppsalaApi.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace UppsalaApi.Models
{
 
    public class CampusInfo : Resource, IEtaggable
    {
        public string Title { get; set; }
        public string Tagline { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public Address Location { get; set; }

        public string GetEtag()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return Md5Hash.ForString(serialized);
        }
    }


    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}