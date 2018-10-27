﻿using System;
namespace UppsalaApi.Models
{
    public class Collection<T> : Resource
    {
        public const string CollectionRelation = "collection";

        public T[] Value { get; set; }
    }
}