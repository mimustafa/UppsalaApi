﻿using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace UppsalaApi.Models
{
    public class Link
    {
        public const string GetMethod = "GET";
        public const string PostMethod = "POST";
        public const string DeleteMethod = "DELETE";

        public static Link To(string routeName, object routeValues = null)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = GetMethod,
                Relations = null
            };

        public static Link ToCollection(string routeName, object routeValues = null)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = GetMethod,
                Relations = new string[] { "collection" }

            };

        public static Link ToForm(
            string routeName,
            object routeValues = null,
            string method = PostMethod,
            params string[] relations)
            => new Link
            {
                RouteName = routeName,
                RouteValues = routeValues,
                Method = method,
                Relations = relations
            };


        [JsonProperty(Order = -4)]
        public string Href { get; set; }

        [JsonProperty(Order = -3, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(GetMethod)]
        public string Method { get; set; }

        [JsonProperty(Order = -2, PropertyName = "rel", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Relations { get; set; }


        // Store the route name before being rewritten
        [JsonIgnore]
        public string RouteName { get; set; }

        // Store the route name before being rewritten
        [JsonIgnore]
        public object RouteValues { get; set; }

    }
}
